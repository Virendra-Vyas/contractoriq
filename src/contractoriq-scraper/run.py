import os
import ssl
import schedule
import time
import logging
import urllib.request
from scrapy.crawler import CrawlerProcess
from scrapy.utils.project import get_project_settings
from dotenv import load_dotenv

load_dotenv()

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s"
)
logger = logging.getLogger(__name__)

API_BASE_URL = os.getenv("API_BASE_URL", "https://localhost:5000")


def trigger_alerts():
    try:
        req = urllib.request.Request(
            f"{API_BASE_URL}/api/alerts/process",
            method="POST",
            headers={"Content-Type": "application/json"}
        )
        ctx = ssl.create_default_context()
        ctx.check_hostname = False
        ctx.verify_mode = ssl.CERT_NONE
        with urllib.request.urlopen(req, context=ctx, timeout=10) as response:
            logger.info(f"Alerts triggered: {response.status}")
    except Exception as e:
        logger.warning(f"Alert trigger failed (non-critical): {e}")


def run_spiders():
    logger.info("Starting scraper run...")
    settings = get_project_settings()
    process = CrawlerProcess(settings)
    process.crawl("reed")
    process.crawl("adzuna")
    process.start()
    logger.info("Scraper run complete.")
    trigger_alerts()


if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1 and sys.argv[1] == "--once":
        run_spiders()
    else:
        run_spiders()
        schedule.every(30).minutes.do(run_spiders)
        while True:
            schedule.run_pending()
            time.sleep(60)