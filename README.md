# Redis.Cache.Extension

Use this library to cache items in Redis cache. This library targets .Net Standard 2.0 spec. Its an extension of StackExchange.Redis to make it easier and simpler to add caching to your .NET applications.

This library configured to support one of the following cache types

- In Memory Cache (default)
- Redis Distributed Cache (opt in)

## Install Package

Install this library in your ASP.NET application.

```
dotnet add package Redis.Cache.Extension
```

## Permissions

### AWS

If you are using this library in your application then make sure you grant appropriate permissions in AWS to reach Systems Manager Parameter Store. There is currently no support to configure cloud provider but that support may be added later.

Add the following IAM permission for your EC2 or Task role. Replace {{env}} with your appropriate environment name.

```
{
   "Version": "2012-10-17",
   "Statement": [
    {
      "Sid": "Grand Systems Manager Parameter Store Access",
      "Effect": "Allow",
      "Action": ["ssm:GetParameter"],
      "Resource": "arn:aws:ssm:*:*:parameter/Redis.Cache.Extension/{{env}}/Redis/ConnectionString"
    }]
}
```

## Configure Your App

### Add following to your Program.cs or Startup.cs file

Register Cache Services

```
builder.Services.AddCache();
```

Use Caching

```
app.UseCache();
```

### Use In Memory Cache

Add the following section in your app settings to use In Memory Cache. Update AbsoluteExpiration to suite your needs.

Sliding expiration is optional and should only be used if cached objects need to slide on hits. Sliding expiration means a span of time within which a cache entry must be accessed before the cache entry is evicted from the cache.

```
"Redis": {
    "Cache": {
        "IsEnabled": "true",
        "AbsoluteExpiration": "01:00:00",
        "SlidingExpiration": "00:05:00" // optional if you want your cached objects to slide on cache hits
    }
}
```

### Use Redis Distributed Cache

Add the following section in your app settings to use Redis Distributed Cache. Update AbsoluteExpiration to suite your needs.

Sliding expiration is optional and should only be used if cached objects need to slide on hits. Sliding expiration means a span of time within which a cache entry must be accessed before the cache entry is evicted from the cache.

```
"Redis": {
    "Cache": {
        "IsEnabled": "true",
        "Type": "Redis",
        "AbsoluteExpiration": "01:00:00",
        "SlidingExpiration": "00:05:00" // optional if you want your cached objects to slide on cache hits
    }
}
```

### Turn Caching Off

Update app settings and change IsEnabled to "false" to disable caching. Restart you application pool for the change to take effect. No need to restart app pool if using [AWS App Config](https://docs.aws.amazon.com/appconfig/latest/userguide/what-is-appconfig.html) with polling enabled.

```
var awsOptions = new AWSOptions()
{
    Region = RegionEndpoint.USEast1
};

builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddAppConfig(
        "YOUR APPLICATION ID",
        "YOUR ENVIRONMENT ID",
        "YOUR CONFIG PROFILE ID",
        awsOptions,
        TimeSpan.FromMinutes(5) // This will poll every 5 mins to check for any changes in app settings
    );
});
```

## How To Use Cache

### Attribute

Use Cache attribute on your controller actions.

```
[HttpGet]
[Cache]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<ActionResult<IEnumerable<string>>> GetAsync()
{
    // Do your operation here

    return Ok(result);
}
```

Use Cache attribute on your controller actions with custom absolute expiration of 12 hours.

```
[HttpGet]
[Cache("12:00:00")]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<ActionResult<IEnumerable<string>>> GetAsync()
{
    // Do your operation here

    return Ok(result);
}
```

Use Cache attribute on your controller actions with custom absolute expiration of 12 hours and custom sliding expiration of 5 hours.

```
[HttpGet]
[Cache("12:00:00", "05:00:00")]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<ActionResult<IEnumerable<string>>> GetAsync()
{
    // Do your operation here

    return Ok(result);
}
```

### Service

Use ICacheService

```
using Redis.Cache.Extension.Services;

namespace YourNameSpace;
public class MyClass
{
    private ICacheService _cache;
    private IRepository _repository;

    public MyClass(ICacheService cache, IRepository repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public GetData(string id)
    {
        var cacheKey = id;
        var cachedData = await _cache.GetAsync(cacheKey);
        if (cachedData == null)
        {
            var data = await _reposiroty.GetDataAsync(id);
            await _cache.SetAsync(cacheKey, data);
        }
    }

    public UpdateData(string id, object data)
    {
        var updatedData = await _reposiroty.UpdateDataAsync(id, data);
        var cacheKey = id;
        await _cache.SetAsync(cacheKey, updatedData);
    }

    public DeleteData(string id)
    {
        var updatedData = await _reposiroty.DeleteDataAsync(id);
        var cacheKey = id;
        await _cache.RemoveAsync(cacheKey, updatedData);
    }
}
```

### Service Method Overloads

ICacheService methods accept cache key as either string or list of strings for all operations. Following example will concatenate list of cache keys to a single cache key string.

```
var cacheKeys = new List<string>()
{
    "plc",
    "brandid",
    "brandname",
    "year",
    "vehicletype",
    "locale"
};
await _cache.GetAsync(cacheKeys);
await _cache.SetAsync(cacheKeys, value);
await _cache.RemoveAsync(cacheKeys);
```

### Service Method Custom Expirations

#### Custom Absolute Expiration

When caching items, you have the option to specify a custom absolute expiration time if you want to cache items for duration different than what is configured in app settings.

```
await _cache.SetAsync(cacheKey, value, new TimeSpan("12:00:00"));
```

#### Custom Sliding Expiration

When caching items, you have the option to specify a custom sliding expiration time if you want to cache items for duration different than what is configured in app settings.

```
await _cache.SetAsync(cacheKey, value, null, new TimeSpan("05:00:00"));
```

#### Custom Absolute and Sliding Expirations

When caching items, you have the option to specify a custom expiration times if you want to cache items for duration different than what is configured in app settings.

```
await _cache.SetAsync(cacheKey, value, new TimeSpan("12:00:00"), new TimeSpan("05:00:00"));
```
