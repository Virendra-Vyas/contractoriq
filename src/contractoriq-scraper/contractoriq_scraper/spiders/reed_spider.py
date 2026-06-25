import os

import scrapy
import re
import json
import logging
from datetime import datetime, timezone
from contractoriq_scraper.items import JobItem

logger = logging.getLogger(__name__)


class ReedSpider(scrapy.Spider):
    name = "reed"
    allowed_domains = ["reed.co.uk"]

    # Reed API endpoint for contract .NET jobs
    base_url = "https://www.reed.co.uk/api/1.0/search"

    custom_settings = {
        "ROBOTSTXT_OBEY": False,  # Reed API doesn't have robots.txt restrictions
        "DOWNLOAD_DELAY": 1,
    }

    def start_requests(self):
        import base64
        api_key = os.getenv("REED_API_KEY", "")
        credentials = base64.b64encode(f"{api_key}:".encode()).decode()

        searches = [
            {"keywords": ".NET developer contract", "locationName": "London"},
            {"keywords": "C# developer contract", "locationName": "London"},
            {"keywords": ".NET developer contract", "locationName": "Remote"},
            {"keywords": "ASP.NET contract", "locationName": "United Kingdom"},
            {"keywords": "React .NET contract", "locationName": "United Kingdom"},
        ]

        for search in searches:
            params = {
                "keywords": search["keywords"],
                "locationName": search["locationName"],
                "contractType": "Contract",
                "resultsToTake": 100,
                "resultsToSkip": 0,
            }
            url = f"{self.base_url}?" + "&".join(f"{k}={v}" for k, v in params.items())
            yield scrapy.Request(
                url=url,
                callback=self.parse,
                headers={
                    "Accept": "application/json",
                    "Authorization": f"Basic {credentials}",
                },
            )

    def parse(self, response):
        try:
            data = json.loads(response.text)
        except json.JSONDecodeError:
            logger.error(f"Failed to parse JSON from {response.url}")
            return

        results = data.get("results", [])
        logger.info(f"Reed returned {len(results)} jobs from {response.url}")

        for job in results:
            item = JobItem()
            item["external_id"] = str(job.get("jobId", ""))
            item["source"] = "reed"
            item["title"] = job.get("jobTitle", "")
            item["company"] = job.get("employerName", "")
            item["location"] = job.get("locationName", "")
            item["is_remote"] = "remote" in job.get("locationName", "").lower()
            item["is_hybrid"] = "hybrid" in job.get("jobTitle", "").lower() or "hybrid" in job.get("locationName", "").lower()
            item["description"] = job.get("jobDescription", "")
            job_title_slug = re.sub(r'[^a-z0-9]+', '-', job.get('jobTitle', '').lower()).strip('-')
            item["source_url"] = f"https://www.reed.co.uk/jobs/{job_title_slug}/{job.get('jobId', '')}"
            item["ir35_status"] = self._extract_ir35(job.get("jobDescription", ""))
            item["tech_stack"] = self._extract_tech_stack(job.get("jobTitle", "") + " " + job.get("jobDescription", ""))
            item["contract_length"] = None
            item["recruiter_name"] = None
            item["recruiter_email"] = None
            item["recruiter_phone"] = None

            # Extract day rate
            min_rate, max_rate = self._extract_day_rate(
                job.get("minimumSalary"),
                job.get("maximumSalary"),
                job.get("jobDescription", "")
            )
            item["day_rate_min"] = min_rate
            item["day_rate_max"] = max_rate

            # Posted date
            date_str = job.get("date", "")
            item["posted_at"] = self._parse_date(date_str)

            yield item

    def _extract_ir35(self, description):
        if not description:
            return "unknown"
        desc_lower = description.lower()
        outside_indicators = ["outside ir35", "outside of ir35", "outside scope"]
        inside_indicators = ["inside ir35", "inside of ir35", "paye", "umbrella only"]
        for indicator in outside_indicators:
            if indicator in desc_lower:
                return "outside"
        for indicator in inside_indicators:
            if indicator in desc_lower:
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
        # If salary fields look like day rates (under 2000)
        if min_salary and min_salary < 2000:
            return float(min_salary), float(max_salary) if max_salary else float(min_salary)

        # Try to extract from description
        patterns = [
            r"£(\d+)\s*-\s*£(\d+)\s*(?:per day|/day|pd)",
            r"£(\d+)\s*(?:per day|/day|pd)",
            r"(\d+)\s*-\s*(\d+)\s*(?:per day|/day|pd)",
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