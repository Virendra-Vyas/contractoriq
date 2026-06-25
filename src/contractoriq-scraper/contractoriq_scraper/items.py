import scrapy


class JobItem(scrapy.Item):
    external_id = scrapy.Field()
    source = scrapy.Field()
    title = scrapy.Field()
    company = scrapy.Field()
    location = scrapy.Field()
    is_remote = scrapy.Field()
    is_hybrid = scrapy.Field()
    day_rate_min = scrapy.Field()
    day_rate_max = scrapy.Field()
    ir35_status = scrapy.Field()
    contract_length = scrapy.Field()
    description = scrapy.Field()
    tech_stack = scrapy.Field()
    recruiter_name = scrapy.Field()
    recruiter_email = scrapy.Field()
    recruiter_phone = scrapy.Field()
    source_url = scrapy.Field()
    posted_at = scrapy.Field()