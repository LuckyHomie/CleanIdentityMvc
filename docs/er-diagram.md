# Diagram ER

Poniższy diagram pokazuje najważniejsze tabele Identity oraz tabele dodane w projekcie.

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : has
    AspNetRoles ||--o{ AspNetUserRoles : has
    AspNetUsers ||--o{ AspNetUserClaims : has
    AspNetUsers ||--o{ AspNetUserLogins : has
    AspNetUsers ||--o{ AspNetUserTokens : has
    AspNetRoles ||--o{ AspNetRoleClaims : has
    AspNetUsers ||--o{ UserActivities : performs
    AspNetUsers ||--o{ PasswordHistories : used
    AspNetUsers ||--o{ LoginAuditLogs : creates

    AspNetUsers {
        string Id PK
        string Email
        string UserName
        string PasswordHash
        int AccessFailedCount
        datetime LockoutEnd
        bool LockoutEnabled
        datetime LastLoginAt
        datetime PasswordChangedAt
        bool ShowActivityAfterLogin
    }

    AspNetRoles {
        string Id PK
        string Name
    }

    AspNetUserRoles {
        string UserId FK
        string RoleId FK
    }

    UserActivities {
        int Id PK
        string UserId FK
        string Action
        string Details
        string IpAddress
        string UserAgent
        datetime CreatedAt
    }

    PasswordHistories {
        int Id PK
        string UserId FK
        string PasswordHash
        datetime CreatedAt
    }

    LoginAuditLogs {
        int Id PK
        string UserId FK
        string Email
        bool Success
        bool LockedOut
        string FailureReason
        string SessionId
        string IpAddress
        string UserAgent
        datetime CreatedAt
        datetime LogoutAt
    }

    AllowedIpAddresses {
        int Id PK
        string Value
        bool Enabled
        string Description
        datetime CreatedAt
    }
```

## Tabele niestandardowe

| Tabela | Cel |
|---|---|
| `UserActivities` | Historia czynności użytkownika widoczna po zalogowaniu. |
| `PasswordHistories` | Ostatnie skróty haseł używane do blokowania powtórzeń. |
| `LoginAuditLogs` | Rejestr prób logowania oraz czasu wylogowania. |
| `AllowedIpAddresses` | Opcjonalna lista adresów IP dopuszczonych do systemu. |

## Diagram przy użyciu Ridera

<img width="2032" height="3416" alt="image" src="https://github.com/user-attachments/assets/8c0248dc-6afa-4cf2-8876-49f10c10dc3d" />
