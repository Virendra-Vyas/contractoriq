import psycopg2
import logging
from datetime import datetime, timezone
from itemadapter import ItemAdapter

logger = logging.getLogger(__name__)


class DatabasePipeline:
    def __init__(self, db_host, db_port, db_name, db_user, db_password):
        self.db_host = db_host
        self.db_port = db_port
        self.db_name = db_name
        self.db_user = db_user
        self.db_password = db_password
        self.conn = None
        self.cursor = None
        self.inserted = 0
        self.skipped = 0

    @classmethod
    def from_crawler(cls, crawler):
        return cls(
            db_host=crawler.settings.get("DB_HOST"),
            db_port=crawler.settings.get("DB_PORT"),
            db_name=crawler.settings.get("DB_NAME"),
            db_user=crawler.settings.get("DB_USER"),
            db_password=crawler.settings.get("DB_PASSWORD"),
        )

    def open_spider(self, spider):
        self.conn = psycopg2.connect(
            host=self.db_host,
            port=self.db_port,
            dbname=self.db_name,
            user=self.db_user,
            password=self.db_password,
        )
        self.cursor = self.conn.cursor()
        logger.info(f"Database connected for spider: {spider.name}")

    def close_spider(self, spider):
        if self.conn:
            self.conn.commit()
            self.cursor.close()
            self.conn.close()
        logger.info(f"Spider {spider.name} finished. Inserted: {self.inserted}, Skipped: {self.skipped}")

    def process_item(self, item, spider):
        adapter = ItemAdapter(item)
        try:
            # Check for duplicate
            self.cursor.execute(
                'SELECT "Id" FROM "Jobs" WHERE "ExternalId" = %s AND "Source" = %s',
                (adapter.get("external_id"), adapter.get("source"))
            )
            if self.cursor.fetchone():
                self.skipped += 1
                return item

            # Insert new job
            self.cursor.execute("""
                INSERT INTO "Jobs" (
                    "Id", "ExternalId", "Source", "Title", "Company",
                    "Location", "IsRemote", "IsHybrid",
                    "DayRateMin", "DayRateMax", "Ir35Status",
                    "ContractLength", "Description", "TechStack",
                    "RecruiterName", "RecruiterEmail", "RecruiterPhone",
                    "SourceUrl", "IsActive", "PostedAt", "ScrapedAt"
                ) VALUES (
                    gen_random_uuid(), %s, %s, %s, %s,
                    %s, %s, %s,
                    %s, %s, %s,
                    %s, %s, %s,
                    %s, %s, %s,
                    %s, true, %s, %s
                )
            """, (
                adapter.get("external_id"),
                adapter.get("source"),
                adapter.get("title"),
                adapter.get("company", ""),
                adapter.get("location", ""),
                adapter.get("is_remote", False),
                adapter.get("is_hybrid", False),
                adapter.get("day_rate_min"),
                adapter.get("day_rate_max"),
                adapter.get("ir35_status", "unknown"),
                adapter.get("contract_length"),
                adapter.get("description", ""),
                adapter.get("tech_stack", ""),
                adapter.get("recruiter_name"),
                adapter.get("recruiter_email"),
                adapter.get("recruiter_phone"),
                adapter.get("source_url", ""),
                adapter.get("posted_at", datetime.now(timezone.utc)),
                datetime.now(timezone.utc),
            ))
            self.conn.commit()
            self.inserted += 1
        except Exception as e:
            self.conn.rollback()
            logger.error(f"Failed to insert job {adapter.get('external_id')}: {e}")
        return item