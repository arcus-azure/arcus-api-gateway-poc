# Arcus - API Gateway POC

POC to integrate Azure API Management with our observability.

![Arcus](https://raw.githubusercontent.com/arcus-azure/arcus/master/media/arcus.png)

## Getting Started

Before you can run this, you need to:

1. Provision an Azure API Management instance with a self-hosted gateway
2. Configure the gateway in Docker Compose
3. Create a Bacon API based on the OpenAPI spec of our local API ([url](http://localhost:789/api/docs/index.html))
4. Make Bacon API available locally
5. Run solution with Docker Compose
6. Get bacon by calling the self-hosted gateway - GET http://localhost:700/api/v1/bacon

## Observability

End-to-end correlation from Azure API Management is provided until the database:

![Overview](media/observability-example.png)

## Action items

- [ ] Be more open and extensible in Arcus
- [ ] Incorporate hacks upstream to be able to interpret, track and use parent id
- [ ] Incorporate hacks upstream allowing users to re-use upstream service operation id and tracking them correspondingly
- [ ] Dependency name
