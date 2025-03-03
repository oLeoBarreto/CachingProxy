
# Caching Proxy

Caching-proxy is CLI tool developed from studies about cache, like a challange from [roadmap.sh](https://roadmap.sh/projects/caching-server) plataform, where it will work listening your localhost and the route that you parameterize, caching the response.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/pt-br/download/visual-studio-sdks) 8.0 or later

## Running

### Clone the project

```bash
  git clone https://github.com/oLeoBarreto/CachingProxy.git
```

Enter in project dir

```bash
  cd Caching-proxy
```

### Docker compose up

To the CLI you firts need a Redis dependence running, you can run the docker componse on project for this!

```bash
  docker compose up
```

### Building project

```bash
  dotnet build
```

### Running project

```bash
  dotnet run --port <port> --origin <origin-url>
```
Example: 
```bash
  dotnet run --port 3000 --origin https://dummyjson.com/  
```

### Clear caching

```bash
  dotnet run --clear-cache    
  
```
## Author

- [@oLeoBarreto](https://github.com/oLeoBarreto)

