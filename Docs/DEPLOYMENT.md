# Deployment Guide - AMS

Comprehensive guide for deploying the AMS to various environments and cloud platforms.

## Deployment Architecture

```
Development (Local Machine)
     ↓
Git Repository (GitHub)
     ↓
CI/CD Pipeline (GitHub Actions/Jenkins)
     ↓
Build & Test Stage
     ↓
Container Image (Docker)
     ↓
Registry (Docker Hub / ECR)
     ↓
QA Environment
     ↓
UAT Environment
     ↓
Production Environment
```

## Pre-Deployment Checklist

- [ ] All tests passing (unit, integration)
- [ ] Code review approved
- [ ] Dependencies updated and reviewed
- [ ] Secrets configured in target environment
- [ ] Database migration scripts tested
- [ ] Load tested in staging
- [ ] Security scan passed
- [ ] Documentation updated
- [ ] Rollback plan prepared
- [ ] Communication sent to stakeholders

## Docker Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["AMS.API/AMS.API.csproj", "AMS.API/"]
COPY ["AMS.Repository/AMS.Repository.csproj", "AMS.Repository/"]
COPY ["AMS.Command/AMS.Command.csproj", "AMS.Command/"]
COPY ["AMS.Query/AMS.Query.csproj", "AMS.Query/"]

RUN dotnet restore "AMS.API/AMS.API.csproj"

COPY . .
WORKDIR "/src/AMS.API"

RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000 7001

ENV ASPNETCORE_URLS=http://+:5000;https://+:7001
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AMS.API.dll"]
```

### Build Docker Image

```bash
# Build image
docker build -t ams-api:1.0.0 .

# Tag for registry
docker tag ams-api:1.0.0 your-registry/ams-api:1.0.0

# Push to registry
docker push your-registry/ams-api:1.0.0
```

### Run Docker Container

```bash
# Run with environment variables
docker run -d \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=db;Port=5432;Database=ams_prod;Username=postgres;Password=***" \
  -e Jwt__SecretKey="your-secret-key" \
  --name ams-api \
  ams-api:1.0.0

# View logs
docker logs -f ams-api

# Stop container
docker stop ams-api
```

### Docker Compose

```yaml
version: '3.8'

services:
  api:
    image: ams-api:1.0.0
    ports:
      - "5000:5000"
      - "7001:7001"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=ams_prod;Username=postgres;Password=postgres"
      Jwt__SecretKey: ${JWT_SECRET_KEY}
    depends_on:
      - db
    networks:
      - ams-network

  db:
    image: postgres:15
    environment:
      POSTGRES_DB: ams_prod
      POSTGRES_PASSWORD: postgres
    volumes:
      - ams-db:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    networks:
      - ams-network

volumes:
  ams-db:

networks:
  ams-network:
```

Run:
```bash
docker-compose up -d
```

## Kubernetes Deployment

### Deployment Manifest (deployment.yaml)

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ams-api
  labels:
    app: ams-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: ams-api
  template:
    metadata:
      labels:
        app: ams-api
    spec:
      containers:
      - name: ams-api
        image: your-registry/ams-api:1.0.0
        ports:
        - containerPort: 5000
          name: http
        - containerPort: 7001
          name: https
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: ams-secrets
              key: db-connection
        - name: Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: ams-secrets
              key: jwt-secret
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: ams-api-service
spec:
  selector:
    app: ams-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 5000
  type: LoadBalancer
```

Deploy:
```bash
kubectl apply -f deployment.yaml
kubectl apply -f secret.yaml

# Check status
kubectl get pods
kubectl get svc ams-api-service
kubectl logs -f deployment/ams-api
```

## AWS Deployment

### ECS (Elastic Container Service)

```bash
# Create ECR repository
aws ecr create-repository --repository-name ams-api

# Login to ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin [ACCOUNT_ID].dkr.ecr.us-east-1.amazonaws.com

# Build and push
docker build -t ams-api:1.0.0 .
docker tag ams-api:1.0.0 [ACCOUNT_ID].dkr.ecr.us-east-1.amazonaws.com/ams-api:1.0.0
docker push [ACCOUNT_ID].dkr.ecr.us-east-1.amazonaws.com/ams-api:1.0.0
```

### RDS Database

```bash
# Create PostgreSQL RDS instance
aws rds create-db-instance \
  --db-instance-identifier ams-prod-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --master-username postgres \
  --master-user-password 'YourPassword123!' \
  --allocated-storage 20 \
  --publicly-accessible false
```

### Environment Variables

Store secrets in AWS Secrets Manager:

```bash
aws secretsmanager create-secret \
  --name ams/prod/db-connection \
  --secret-string "Host=ams-prod-db.xxx.rds.amazonaws.com;Port=5432;Database=ams_prod;Username=postgres;Password=***"

aws secretsmanager create-secret \
  --name ams/prod/jwt-secret \
  --secret-string "your-super-secret-key-here"
```

## Database Migration Strategy

### Pre-Deployment

1. **Test migrations in staging**
   ```bash
   # Apply migrations to staging database
   dotnet run --environment Staging
   # Verify tables and data integrity
   ```

2. **Backup production database**
   ```bash
   pg_dump -h [prod-host] -U postgres -d ams_prod > ams_backup.sql
   gzip ams_backup.sql
   ```

3. **Create migration rollback script**
   ```sql
   -- 005_Rollback.sql (if needed)
   DROP TABLE IF EXISTS new_table;
   ALTER TABLE old_table ADD COLUMN restored_column VARCHAR(100);
   ```

### Deployment

- Migrations auto-run on application startup
- Monitor `schemaversions` table for completion
- Check application logs for migration status

### Rollback

1. Stop application
2. Restore database from backup
3. Revert application version
4. Start application

## Zero-Downtime Deployment

### Blue-Green Deployment

```
Production (Blue):
  - Current version v1.0
  - Serving all traffic
  ↓
Staging (Green):
  - Deploy new version v1.1
  - Run tests, smoke tests
  ↓
Switch Traffic:
  - Load balancer → Green (v1.1)
  - Blue (v1.0) ready for rollback
  ↓
Keep Blue running for 24h (quick rollback if needed)
```

### Rolling Deployment (Kubernetes)

```yaml
spec:
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1        # One extra pod during update
      maxUnavailable: 0  # Never bring down serving pods
```

## Health Checks

### Health Endpoint

```csharp
app.MapGet("/health", async (IUnitOfWork unitOfWork) =>
{
    try
    {
        // Check database connectivity
        var employee = await unitOfWork.Employees.GetByIdAsync(1);
        
        return new { status = "healthy", timestamp = DateTime.UtcNow };
    }
    catch
    {
        return Results.StatusCode(503);  // Service Unavailable
    }
});
```

**Configure in load balancer**:
- Path: `/health`
- Interval: 30 seconds
- Timeout: 10 seconds
- Healthy threshold: 2 consecutive checks
- Unhealthy threshold: 3 consecutive failures

## Performance & Load Testing

### Load Test with Apache Bench

```bash
# 1000 requests from 10 concurrent clients
ab -n 1000 -c 10 http://localhost:5000/api/employees

# With authentication token
ab -n 1000 -c 10 -H "Authorization: Bearer TOKEN" \
  http://localhost:5000/api/employees
```

### Load Test with k6

```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  vus: 10,      // 10 virtual users
  duration: '30s',
};

export default function () {
  let response = http.get('http://localhost:5000/api/employees');
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
}
```

Run:
```bash
k6 run load-test.js
```

## Monitoring & Logging

### CloudWatch Setup

```csharp
builder.Services.AddCloudWatchLogging(builder.Configuration);
```

**View logs**:
```bash
aws logs get-log-events \
  --log-group-name "/ams/api/production" \
  --log-stream-name "prod-server"
```

### Application Insights (Azure)

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Datadog Integration

```csharp
builder.Services.AddDatadog(builder.Configuration);
```

## Security in Production

- [ ] HTTPS enforced (HTTP → HTTPS redirect)
- [ ] Secrets not in environment (use Secrets Manager)
- [ ] Database backups encrypted
- [ ] Regular security patches applied
- [ ] WAF (Web Application Firewall) enabled
- [ ] DDoS protection enabled
- [ ] VPC/Network security configured
- [ ] API rate limiting enabled
- [ ] Authentication/authorization enforced
- [ ] Audit logging enabled

## Rollback Procedure

### Automated Rollback (if using Blue-Green)

```bash
# Switch traffic back to Blue
aws elb set-instance-health \
  --instances i-xxxxx \
  --state InService

aws autoscaling update-auto-scaling-group \
  --auto-scaling-group-name ams-asg-v1.0 \
  --desired-capacity 3
```

### Manual Rollback

1. **Identify issue**
   ```bash
   kubectl logs deployment/ams-api | tail -100
   ```

2. **Rollback deployment**
   ```bash
   kubectl rollout undo deployment/ams-api
   ```

3. **Verify status**
   ```bash
   kubectl get pods
   kubectl get svc
   ```

4. **Check application logs**
   ```bash
   kubectl logs -f deployment/ams-api
   ```

## Post-Deployment Validation

- [ ] Health checks passing
- [ ] API responding to requests
- [ ] Database queries working
- [ ] Authentication functional
- [ ] Log messages appearing
- [ ] Metrics/monitoring active
- [ ] No error spikes in logs
- [ ] Performance baseline met
- [ ] User-facing functionality verified
- [ ] Integrations with external services working

## Maintenance Windows

Schedule maintenance with minimal user impact:

```
Friday 2:00 AM - 3:00 AM UTC
- Database backups
- Log rotation
- Log aggregation cleanup
```

Communicate:
- Email notification 1 week before
- In-app notification 24 hours before
- Status page update during maintenance

## Deployment Checklist

### Development → Staging
- [ ] Code review completed
- [ ] All tests passing
- [ ] Deployed to staging
- [ ] Smoke tests passed
- [ ] Database migrations verified
- [ ] Configuration loaded correctly
- [ ] Health checks responding

### Staging → Production
- [ ] Production database backed up
- [ ] Load test successful
- [ ] Security scan passed
- [ ] Stakeholders notified
- [ ] Rollback plan prepared
- [ ] Deployment window approved
- [ ] On-call engineer assigned
- [ ] Deployment completed
- [ ] Post-deployment validation passed
- [ ] Status page updated

---

**Last Updated**: March 8, 2024  
**Deployment Version**: 1.0  
**RTO Target**: 15 minutes  
**RPO Target**: 1 hour
