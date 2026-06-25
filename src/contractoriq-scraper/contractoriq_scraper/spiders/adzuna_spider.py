import scrapy
import json
import logging
import os
import re
from datetime import datetime, timezone
from contractoriq_scraper.items import JobItem

logger = logging.getLogger(__name__)


class AdzunaSpider(scrapy.Spider):
    name = "adzuna"
    allowed_domains = ["api.adzuna.com"]

    custom_settings = {
        "DOWNLOAD_DELAY": 1,
        "ROBOTSTXT_OBEY": False,
    }

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.app_id = os.getenv("ADZUNA_APP_ID", "")
        self.app_key = os.getenv("ADZUNA_APP_KEY", "")

    def start_requests(self):
        searches = [
            ".NET developer contract",
            "CSharp developer contract",
            "ASP.NET contract",
            "React .NET contract",
            "software engineer contract outside IR35",
            "Full stack developer contract",
        ]

        for term in searches:
            for page in range(1, 3):  # pages 1 and 2
                url = (
                    f"https://api.adzuna.com/v1/api/jobs/gb/search/{page}"
                    f"?app_id={self.app_id}"
                    f"&app_key={self.app_key}"
                    f"&results_per_page=50"
                    f"&what={term.replace(' ', '%20')}"
                    f"&contract=1"
                    f"&content-type=application/json"
                )
                yield scrapy.Request(
                    url=url,
                    callback=self.parse,
                    meta={"search_term": term},
                )

    def parse(self, response):
        try:
            data = json.loads(response.text)
        except json.JSONDecodeError:
            logger.error(f"Failed to parse JSON from {response.url}")
            return

        results = data.get("results", [])
        logger.info(f"Adzuna returned {len(results)} jobs for '{response.meta['search_term']}'")

        for job in results:
            item = JobItem()
            item["external_id"] = str(job.get("id", ""))
            item["source"] = "adzuna"
            item["title"] = job.get("title", "")
            item["company"] = job.get("company", {}).get("display_name", "")
            item["location"] = job.get("location", {}).get("display_name", "")
            item["description"] = job.get("description", "")
            item["source_url"] = job.get("redirect_url", "")
            item["contract_length"] = None
            item["recruiter_name"] = None
            item["recruiter_email"] = None
            item["recruiter_phone"] = None

            # Remote/hybrid detection
            desc_lower = (job.get("description", "") + " " + job.get("title", "")).lower()
            item["is_remote"] = "remote" in desc_lower or "fully remote" in desc_lower
            item["is_hybrid"] = "hybrid" in desc_lower

            # IR35
            item["ir35_status"] = self._extract_ir35(job.get("description", ""))

            # Tech stack
            item["tech_stack"] = self._extract_tech_stack(
                job.get("title", "") + " " + job.get("description", "")
            )

            # Day rate
            min_rate, max_rate = self._extract_day_rate(
                job.get("salary_min"),
                job.get("salary_max"),
                job.get("description", "")
            )
            item["day_rate_min"] = min_rate
            item["day_rate_max"] = max_rate

            # Posted date
            created = job.get("created", "")
            item["posted_at"] = self._parse_date(created)

            yield item

    def _extract_ir35(self, description):
        if not description:
            return "unknown"
        desc_lower = description.lower()
        if any(x in desc_lower for x in ["outside ir35", "outside of ir35"]):
            return "outside"
        if any(x in desc_lower for x in ["inside ir35", "inside of ir35", "paye", "umbrella only"]):
            return "inside"
        return "unknown"

    def _extract_tech_stack(self, text):
        if not text:
            return ""
        tech_keywords = [
            ".NET", "C#", "ASP.NET", "React", "TypeScript", "JavaScript",
            "SQL Server", "PostgreSQL", "Azure", "AWS", "Docker", "Kubernetes",
            "Entity Framework", "REST API", "Microservices", "Angular", "Vue",
            "Python", "Java", "Node.js", "DevOps", "CI/CD", "Git",
        ]
        found = []
        text_upper = text.upper()
        for tech in tech_keywords:
            if tech.upper() in text_upper and tech not in found:
                found.append(tech)
        return ",".join(found[:10])

    def _extract_day_rate(self, min_salary, max_salary, description):
        # Adzuna returns annual salary — convert if looks like day rate
        if min_salary and min_salary < 2000:
            return float(min_salary), float(max_salary) if max_salary else float(min_salary)

        # Try to extract from description
        patterns = [
            r"£(\d+)\s*-\s*£(\d+)\s*(?:per day|/day|pd|p/d)",
            r"£(\d+)\s*(?:per day|/day|pd|p/d)",
            r"(\d{3,4})\s*-\s*(\d{3,4})\s*(?:per day|/day|pd|p/d)",
        ]
        for pattern in patterns:
            match = re.search(pattern, description, re.IGNORECASE)
            if match:
                groups = match.groups()
                if len(groups) == 2 and groups[1]:
                    return float(groups[0]), float(groups[1])
                return float(groups[0]), float(groups[0])
        return None, None

    def _parse_date(self, date_str):
        if not date_str:
            return datetime.now(timezone.utc)
        try:
            return datetime.fromisoformat(date_str.replace("Z", "+00:00"))
        except (ValueError, AttributeError):
            return datetime.now(timezone.utc)