## Introduction

This document provides comprehensive component diagrams for the VAT Filing Pricing Tool, illustrating the system's architectural components, their relationships, responsibilities, and interactions. These diagrams help developers, architects, and other stakeholders understand the structure of the system and how different components work together to provide the required functionality.

The component diagrams follow a hierarchical approach, starting with a high-level system context diagram, then drilling down into container diagrams, component diagrams, and finally detailed component diagrams for key subsystems. This approach is inspired by the C4 model for visualizing software architecture.

The VAT Filing Pricing Tool is designed as a cloud-native application that provides businesses with accurate cost estimates for VAT filing services across multiple jurisdictions. The system addresses the complex challenge of determining VAT filing costs based on various factors including business size, transaction volume, filing frequency, and country-specific tax regulations.

## System Context Diagram
The system context diagram shows the VAT Filing Pricing Tool as a whole and its interactions with external users and systems.
```mermaid
graph TD
    subgraph "External Users"
        A[Finance Teams] --> VAT[VAT Filing Pricing Tool]
        B[Accountants] --> VAT
        C[Business Owners] --> VAT
        D[Tax Consultants] --> VAT
    end
    
    subgraph "External Systems"
        VAT --> E[Microsoft Dynamics 365]
        VAT --> F[Other ERP Systems]
        VAT --> G[Azure Active Directory]
        VAT --> H[Azure Cognitive Services]
        VAT --> I[Email Services]
    end
    
    style VAT fill:#1168bd,stroke:#0b4884,color:#ffffff
    style A fill:#08427b,stroke:#052E56,color:#ffffff
    style B fill:#08427b,stroke:#052E56,color:#ffffff
    style C fill:#08427b,stroke:#052E56,color:#ffffff
    style D fill:#08427b,stroke:#052E56,color:#ffffff
    style E fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style F fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style G fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style H fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style I fill:#5a9a08,stroke:#3F6C06,color:#ffffff
```
## Container Diagram
The container diagram shows the high-level technical components (containers) that make up the VAT Filing Pricing Tool system.
```mermaid
graph TD
    subgraph "VAT Filing Pricing Tool"
        A[Client Application<br>Blazor WebAssembly] --> B[API Gateway<br>Azure API Management]
        
        B --> C[Authentication Service<br>ASP.NET Core]
        B --> D[Pricing Engine<br>ASP.NET Core]
        B --> E[Rules Engine<br>ASP.NET Core]
        B --> F[Reporting Service<br>ASP.NET Core]
        B --> G[Integration Service<br>ASP.NET Core]
        B --> H[Admin Service<br>ASP.NET Core]
        
        C --> I[(User Database<br>Azure SQL)]
        D --> J[(Pricing Database<br>Azure SQL)]
        E --> K[(Rules Database<br>Cosmos DB)]
        F --> L[(Report Storage<br>Blob Storage)]
        G --> M[(Integration Database<br>Azure SQL)]
        H --> I
        H --> K
        
        N[Background Services<br>Azure Functions] --> J
        N --> K
        N --> L
    end
    
    O[Azure Active Directory] --> C
    P[ERP Systems] --> G
    Q[Azure Cognitive Services] --> G
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#1168bd,stroke:#0b4884,color:#ffffff
    style C fill:#1168bd,stroke:#0b4884,color:#ffffff
    style D fill:#1168bd,stroke:#0b4884,color:#ffffff
    style E fill:#1168bd,stroke:#0b4884,color:#ffffff
    style F fill:#1168bd,stroke:#0b4884,color:#ffffff
    style G fill:#1168bd,stroke:#0b4884,color:#ffffff
    style H fill:#1168bd,stroke:#0b4884,color:#ffffff
    style N fill:#1168bd,stroke:#0b4884,color:#ffffff
    style I fill:#438dd5,stroke:#2E6295,color:#ffffff
    style J fill:#438dd5,stroke:#2E6295,color:#ffffff
    style K fill:#438dd5,stroke:#2E6295,color:#ffffff
    style L fill:#438dd5,stroke:#2E6295,color:#ffffff
    style M fill:#438dd5,stroke:#2E6295,color:#ffffff
    style O fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style P fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style Q fill:#5a9a08,stroke:#3F6C06,color:#ffffff
```
## Component Diagrams
The following component diagrams provide a more detailed view of each major subsystem within the VAT Filing Pricing Tool.
### Client Application Components
The client application is a Blazor WebAssembly single-page application (SPA) with the following components:
```mermaid
graph TD
    subgraph "Client Application"
        A[App Shell] --> B[Authentication Module]
        A --> C[Dashboard Module]
        A --> D[Pricing Calculator Module]
        A --> E[Reporting Module]
        A --> F[User Profile Module]
        A --> G[Admin Module]
        
        B --> B1[Login Component]
        B --> B2[Registration Component]
        B --> B3[Password Reset Component]
        
        C --> C1[Activity Summary Component]
        C --> C2[Recent Estimates Component]
        C --> C3[Quick Actions Component]
        C --> C4[Notifications Component]
        
        D --> D1[Country Selector Component]
        D --> D2[Service Type Selector Component]
        D --> D3[Transaction Volume Component]
        D --> D4[Additional Services Component]
        D --> D5[Pricing Results Component]
        
        E --> E1[Report Generator Component]
        E --> E2[Report List Component]
        E --> E3[Report Viewer Component]
        
        F --> F1[Profile Editor Component]
        F --> F2[Settings Component]
        
        G --> G1[User Management Component]
        G --> G2[Rule Configuration Component]
        G --> G3[Country Configuration Component]
        G --> G4[Audit Log Component]
        
        H[API Client Service] --> B
        H --> C
        H --> D
        H --> E
        H --> F
        H --> G
        
        I[Authentication State Provider] --> H
        J[Local Storage Service] --> I
    end
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#85bbf0,stroke:#5D82A8,color:#000000
```
### Authentication Service Components
The Authentication Service manages user identity and access control with the following components:
```mermaid
graph TD
    subgraph "Authentication Service"
        A[Authentication Controller] --> B[Authentication Service]
        A --> C[User Controller]
        
        B --> D[Token Service]
        B --> E[Azure AD Authentication Handler]
        B --> F[Password Service]
        
        C --> G[User Service]
        
        D --> H[JWT Token Handler]
        
        G --> I[User Repository]
        
        I --> J[(User Database)]
    end
    
    K[Azure Active Directory] --> E
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#1168bd,stroke:#0b4884,color:#ffffff
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#438dd5,stroke:#2E6295,color:#ffffff
    style K fill:#5a9a08,stroke:#3F6C06,color:#ffffff
```
### Pricing Engine Components
The Pricing Engine calculates VAT filing costs based on multiple parameters with the following components:
```mermaid
graph TD
    subgraph "Pricing Engine"
        A[Pricing Controller] --> B[Pricing Service]
        
        B --> C[Calculation Service]
        B --> D[Country Service]
        B --> E[Service Type Service]
        
        C --> F[Pricing Calculator]
        C --> G[Discount Calculator]
        C --> H[Additional Services Calculator]
        
        F --> I[Rules Engine Client]
        
        B --> J[Calculation Repository]
        D --> K[Country Repository]
        E --> L[Service Repository]
        
        J --> M[(Pricing Database)]
        K --> M
        L --> M
        
        N[Redis Cache Service] --> B
    end
    
    I --> O[Rules Engine]
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#85bbf0,stroke:#5D82A8,color:#000000
    style K fill:#85bbf0,stroke:#5D82A8,color:#000000
    style L fill:#85bbf0,stroke:#5D82A8,color:#000000
    style M fill:#438dd5,stroke:#2E6295,color:#ffffff
    style N fill:#85bbf0,stroke:#5D82A8,color:#000000
    style O fill:#1168bd,stroke:#0b4884,color:#ffffff
```
### Rules Engine Components
The Rules Engine manages and applies country-specific VAT rules with the following components:
```mermaid
graph TD
    subgraph "Rules Engine"
        A[Rules Controller] --> B[Rules Service]
        
        B --> C[Rule Evaluator]
        B --> D[Expression Parser]
        B --> E[Rule Validator]
        
        C --> F[Expression Evaluator]
        
        B --> G[Rule Repository]
        
        G --> H[(Rules Database<br>Cosmos DB)]
        
        I[Redis Cache Service] --> B
    end
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#438dd5,stroke:#2E6295,color:#ffffff
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
```
### Reporting Service Components
The Reporting Service generates and manages reports with the following components:
```mermaid
graph TD
    subgraph "Reporting Service"
        A[Report Controller] --> B[Report Service]
        
        B --> C[Report Generator]
        B --> D[Template Service]
        B --> E[Export Service]
        
        C --> F[PDF Generator]
        C --> G[Excel Generator]
        
        B --> H[Report Repository]
        D --> I[Template Repository]
        
        H --> J[(Report Database)]
        
        E --> K[Blob Storage Client]
        I --> K
        
        K --> L[(Blob Storage)]
    end
    
    B --> M[Pricing Engine Client]
    M --> N[Pricing Engine]
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#438dd5,stroke:#2E6295,color:#ffffff
    style K fill:#85bbf0,stroke:#5D82A8,color:#000000
    style L fill:#438dd5,stroke:#2E6295,color:#ffffff
    style M fill:#85bbf0,stroke:#5D82A8,color:#000000
    style N fill:#1168bd,stroke:#0b4884,color:#ffffff
```
### Integration Service Components
The Integration Service facilitates data exchange with external systems with the following components:
```mermaid
graph TD
    subgraph "Integration Service"
        A[Integration Controller] --> B[Integration Service]
        
        B --> C[ERP Connector]
        B --> D[OCR Processor]
        B --> E[Email Service]
        
        C --> F[Dynamics Connector]
        C --> G[Generic ERP Connector]
        
        D --> H[Azure Cognitive Services Client]
        
        B --> I[Integration Repository]
        
        I --> J[(Integration Database)]
        
        K[Blob Storage Client] --> D
        K --> L[(Blob Storage)]
    end
    
    F --> M[Microsoft Dynamics 365]
    G --> N[Other ERP Systems]
    H --> O[Azure Cognitive Services]
    E --> P[Azure Communication Services]
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#438dd5,stroke:#2E6295,color:#ffffff
    style K fill:#85bbf0,stroke:#5D82A8,color:#000000
    style L fill:#438dd5,stroke:#2E6295,color:#ffffff
    style M fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style N fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style O fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style P fill:#5a9a08,stroke:#3F6C06,color:#ffffff
```
### Admin Service Components
The Admin Service provides administrative functionality with the following components:
```mermaid
graph TD
    subgraph "Admin Service"
        A[Admin Controller] --> B[Admin Service]
        
        B --> C[User Management Service]
        B --> D[Rule Management Service]
        B --> E[Country Management Service]
        B --> F[System Configuration Service]
        B --> G[Audit Service]
        
        C --> H[User Repository]
        D --> I[Rule Repository]
        E --> J[Country Repository]
        F --> K[Configuration Repository]
        G --> L[Audit Repository]
        
        H --> M[(User Database)]
        I --> N[(Rules Database)]
        J --> O[(Country Database)]
        K --> P[(Configuration Database)]
        L --> Q[(Audit Database)]
    end
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#85bbf0,stroke:#5D82A8,color:#000000
    style K fill:#85bbf0,stroke:#5D82A8,color:#000000
    style L fill:#85bbf0,stroke:#5D82A8,color:#000000
    style M fill:#438dd5,stroke:#2E6295,color:#ffffff
    style N fill:#438dd5,stroke:#2E6295,color:#ffffff
    style O fill:#438dd5,stroke:#2E6295,color:#ffffff
    style P fill:#438dd5,stroke:#2E6295,color:#ffffff
    style Q fill:#438dd5,stroke:#2E6295,color:#ffffff
```
## Cross-Cutting Components
The following components are used across multiple services in the VAT Filing Pricing Tool:
### Logging and Monitoring Components
Components for logging, monitoring, and observability:
```mermaid
graph TD
    subgraph "Logging and Monitoring"
        A[Application Insights] --> B[Log Analytics]
        C[Azure Monitor] --> B
        
        D[Logging Service] --> A
        E[Metrics Service] --> C
        
        F[Alert Service] --> B
        F --> G[Notification Service]
        
        H[Dashboard Service] --> B
    end
    
    I[Microservices] --> D
    I --> E
    
    style A fill:#85bbf0,stroke:#5D82A8,color:#000000
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#1168bd,stroke:#0b4884,color:#ffffff
```
### Security Components
Components for security and data protection:
```mermaid
graph TD
    subgraph "Security Components"
        A[Authentication Middleware] --> B[JWT Token Handler]
        A --> C[Azure AD Authentication Handler]
        
        D[Authorization Middleware] --> E[Role-based Authorization]
        D --> F[Resource-based Authorization]
        D --> G[Claims-based Authorization]
        
        H[Data Protection] --> I[Encryption Service]
        H --> J[Key Vault Client]
        
        K[Security Monitoring] --> L[Audit Service]
        K --> M[Threat Detection Service]
    end
    
    N[Microservices] --> A
    N --> D
    N --> H
    N --> K
    
    J --> O[Azure Key Vault]
    
    style A fill:#85bbf0,stroke:#5D82A8,color:#000000
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#85bbf0,stroke:#5D82A8,color:#000000
    style K fill:#85bbf0,stroke:#5D82A8,color:#000000
    style L fill:#85bbf0,stroke:#5D82A8,color:#000000
    style M fill:#85bbf0,stroke:#5D82A8,color:#000000
    style N fill:#1168bd,stroke:#0b4884,color:#ffffff
    style O fill:#5a9a08,stroke:#3F6C06,color:#ffffff
```
### Common Infrastructure Components
Shared infrastructure components used across services:
```mermaid
graph TD
    subgraph "Common Infrastructure"
        A[Configuration Service] --> B[Azure App Configuration]
        A --> C[Environment Variables]
        
        D[Cache Service] --> E[Redis Cache]
        
        F[Resilience Service] --> G[Circuit Breaker]
        F --> H[Retry Policy]
        F --> I[Timeout Policy]
        
        J[Validation Service] --> K[Model Validators]
        J --> L[Business Rule Validators]
    end
    
    M[Microservices] --> A
    M --> D
    M --> F
    M --> J
    
    style A fill:#85bbf0,stroke:#5D82A8,color:#000000
    style B fill:#85bbf0,stroke:#5D82A8,color:#000000
    style C fill:#85bbf0,stroke:#5D82A8,color:#000000
    style D fill:#85bbf0,stroke:#5D82A8,color:#000000
    style E fill:#85bbf0,stroke:#5D82A8,color:#000000
    style F fill:#85bbf0,stroke:#5D82A8,color:#000000
    style G fill:#85bbf0,stroke:#5D82A8,color:#000000
    style H fill:#85bbf0,stroke:#5D82A8,color:#000000
    style I fill:#85bbf0,stroke:#5D82A8,color:#000000
    style J fill:#85bbf0,stroke:#5D82A8,color:#000000
    style K fill:#85bbf0,stroke:#5D82A8,color:#000000
    style L fill:#85bbf0,stroke:#5D82A8,color:#000000
    style M fill:#1168bd,stroke:#0b4884,color:#ffffff
```
## Component Interaction Patterns
The VAT Filing Pricing Tool implements several service interaction patterns to ensure efficient communication between components:
### Request-Response Pattern
Used for synchronous operations where an immediate response is required:
```mermaid
sequenceDiagram
    participant Client
    participant API as API Gateway
    participant Auth as Authentication Service
    participant Pricing as Pricing Engine
    participant Rules as Rules Engine
    
    Client->>API: Calculate Price Request
    API->>Auth: Validate Token
    Auth-->>API: Token Valid
    
    API->>Pricing: Forward Request
    Pricing->>Rules: Get Applicable Rules
    Rules-->>Pricing: Return Rules
    
    Pricing->>Pricing: Calculate Price
    Pricing-->>API: Return Result
    API-->>Client: Return Formatted Response
```
### Event-Driven Pattern
Used for asynchronous operations and notifications:
```mermaid
sequenceDiagram
    participant Admin
    participant AdminSvc as Admin Service
    participant EventGrid as Azure Event Grid
    participant Rules as Rules Engine
    participant Pricing as Pricing Engine
    participant Cache as Redis Cache
    
    Admin->>AdminSvc: Update VAT Rule
    AdminSvc->>Rules: Update Rule
    Rules-->>AdminSvc: Rule Updated
    
    Rules->>EventGrid: Publish RuleChanged Event
    
    EventGrid->>Pricing: Notify Rule Change
    Pricing->>Cache: Invalidate Cached Calculations
    
    EventGrid->>Rules: Notify for Audit
    Rules->>Rules: Log Audit Entry
```
### Saga Pattern
Used for distributed transactions that span multiple services:
```mermaid
sequenceDiagram
    participant Client
    participant API as API Gateway
    participant Report as Reporting Service
    participant Pricing as Pricing Engine
    participant Storage as Blob Storage
    participant Notify as Notification Service
    
    Client->>API: Generate Complex Report
    API->>Report: Create Report Request
    Report->>Report: Create Report Record
    
    Report->>Pricing: Request Calculation Data
    Pricing-->>Report: Return Calculation Data
    
    Report->>Report: Generate Report Content
    Report->>Storage: Store Report File
    
    alt Success
        Storage-->>Report: Storage Success
        Report->>Report: Update Report Status
        Report->>Notify: Send Completion Notification
        Notify-->>Client: Notify User
        Report-->>API: Return Success
        API-->>Client: Report Ready
    else Failure
        Storage-->>Report: Storage Failed
        Report->>Report: Revert Report Record
        Report->>Notify: Send Failure Notification
        Notify-->>Client: Notify User
        Report-->>API: Return Error
        API-->>Client: Report Failed
    end
```
### Cache-Aside Pattern
Used for improving performance by caching frequently accessed data:
```mermaid
sequenceDiagram
    participant Client
    participant API as API Gateway
    participant Service as Service
    participant Cache as Redis Cache
    participant DB as Database
    
    Client->>API: Request Data
    API->>Service: Forward Request
    Service->>Cache: Check Cache
    
    alt Cache Hit
        Cache-->>Service: Return Cached Data
        Service-->>API: Return Data
        API-->>Client: Return Formatted Response
    else Cache Miss
        Cache-->>Service: Cache Miss
        Service->>DB: Query Database
        DB-->>Service: Return Data
        Service->>Cache: Store in Cache
        Service-->>API: Return Data
        API-->>Client: Return Formatted Response
    end
```
## Component Dependencies
The following diagram illustrates the dependencies between major components in the VAT Filing Pricing Tool:
```mermaid
graph TD
    A[Client Application] --> B[API Gateway]
    
    B --> C[Authentication Service]
    B --> D[Pricing Engine]
    B --> E[Rules Engine]
    B --> F[Reporting Service]
    B --> G[Integration Service]
    B --> H[Admin Service]
    
    D --> E
    F --> D
    
    C --> I[Azure Active Directory]
    G --> J[ERP Systems]
    G --> K[OCR Service]
    
    L[Background Services] --> D
    L --> F
    L --> G
    
    M[Redis Cache] --> C
    M --> D
    M --> E
    M --> F
    M --> G
    M --> H
    
    N[Blob Storage] --> F
    N --> G
    
    O[SQL Database] --> C
    O --> D
    O --> F
    O --> G
    O --> H
    
    P[Cosmos DB] --> E
    P --> H
    
    style A fill:#1168bd,stroke:#0b4884,color:#ffffff
    style B fill:#1168bd,stroke:#0b4884,color:#ffffff
    style C fill:#1168bd,stroke:#0b4884,color:#ffffff
    style D fill:#1168bd,stroke:#0b4884,color:#ffffff
    style E fill:#1168bd,stroke:#0b4884,color:#ffffff
    style F fill:#1168bd,stroke:#0b4884,color:#ffffff
    style G fill:#1168bd,stroke:#0b4884,color:#ffffff
    style H fill:#1168bd,stroke:#0b4884,color:#ffffff
    style I fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style J fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style K fill:#5a9a08,stroke:#3F6C06,color:#ffffff
    style L fill:#1168bd,stroke:#0b4884,color:#ffffff
    style M fill:#438dd5,stroke:#2E6295,color:#ffffff
    style N fill:#438dd5,stroke:#2E6295,color:#ffffff
    style O fill:#438dd5,stroke:#2E6295,color:#ffffff
    style P fill:#438dd5,stroke:#2E6295,color:#ffffff
```
## Deployment View
The following diagram illustrates how the components are deployed on Azure infrastructure: