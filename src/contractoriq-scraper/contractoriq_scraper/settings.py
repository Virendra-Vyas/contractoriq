import os
from dotenv import load_dotenv

load_dotenv()

BOT_NAME = "contractoriq_scraper"
SPIDER_MODULES = ["contractoriq_scraper.spiders"]
NEWSPIDER_MODULE = "contractoriq_scraper.spiders"

# Respect robots.txt
ROBOTSTXT_OBEY = True

# Throttle requests - be polite to job boards
DOWNLOAD_DELAY = 2
RANDOMIZE_DOWNLOAD_DELAY = True
AUTOTHROTTLE_ENABLED = True
AUTOTHROTTLE_START_DELAY = 1
AUTOTHROTTLE_MAX_DELAY = 10
AUTOTHROTTLE_TARGET_CONCURRENCY = 1.0

# Concurrent requests
CONCURRENT_REQUESTS = 4
CONCURRENT_REQUESTS_PER_DOMAIN = 2

# Retry
RETRY_TIMES = 3
RETRY_HTTP_CODES = [500, 502, 503, 504, 429]

# User agent rotation
DOWNLOADER_MIDDLEWARES = {
    "contractoriq_scraper.middlewares.RandomUserAgentMiddleware": 400,
    "scrapy.downloadermiddlewares.useragent.UserAgentMiddleware": None,
}

# Item pipelines
ITEM_PIPELINES = {
    "contractoriq_scraper.pipelines.DatabasePipeline": 300,
}

# Database settings from env
DB_HOST = os.getenv("DB_HOST", "127.0.0.1")
DB_PORT = int(os.getenv("DB_PORT", "5433"))
DB_NAME = os.getenv("DB_NAME", "contractoriq")
DB_USER = os.getenv("DB_USER", "contractoriq")
DB_PASSWORD = os.getenv("DB_PASSWORD", "contractoriq123")

# Request fingerprinting
REQUEST_FINGERPRINTER_IMPLEMENTATION = "2.7"
TWISTED_REACTOR = "twisted.internet.asyncioreactor.AsyncioSelectorReactor"
FEED_EXPORT_ENCODING = "utf-8"

LOG_LEVEL = "INFO"

ADZUNA_APP_ID = os.getenv("ADZUNA_APP_ID", "fa6d4e38")
ADZUNA_APP_KEY = os.getenv("ADZUNA_APP_KEY", "5d3ff61d700faf8821d4da4f8d885e0b")