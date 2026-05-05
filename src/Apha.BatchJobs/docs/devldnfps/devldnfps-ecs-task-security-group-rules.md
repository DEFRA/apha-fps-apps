# DEVLDNFPS ECS Task Security Group Rules

This rule set mirrors your LLD baseline and adds implementation details needed for enforceable rules.

## Security Group
- Name: devldnfps-ecs-task-sg
- Scope: ECS tasks for FPS API and Batch Jobs

## Inbound
| Source | Protocol | Port | Purpose |
|---|---|---:|---|
| app-alb-sg | TCP | 8080 | Allow app traffic from ALB only |

## Outbound
| Destination | Protocol | Port | Purpose |
|---|---|---:|---|
| devldnfps-rds-sg | TCP | 5432 | PostgreSQL connectivity |
| devldnfps-redis-sg | TCP | 6379 | Redis connectivity |
| Approved egress path (FortiGate route) | TCP | 443 | Outbound HTTPS to Azure Entra and required external endpoints |

## Implementation Notes
1. Prefer SG-to-SG rules instead of CIDR where possible.
2. Do not keep 0.0.0.0/0 inbound on task SG.
3. If private subnets are used, confirm NAT or firewall route for HTTPS egress.
4. Keep ALB SG and ECS task SG separate for least privilege.

## Validation
1. ECS health checks succeed through ALB on port 8080.
2. App can open DB connection on 5432.
3. App can connect to Redis on 6379.
4. Azure Entra auth flows succeed via outbound 443 path.
