# Database Design

## Database

SQLite

---

# Customer

| Column | Type | Description |
|---------|------|-------------|
| id | INTEGER PK | 고객 ID |
| name | TEXT | 이름 |
| phone | TEXT | 연락처 |
| birthday | DATE | 생년월일 |
| status | TEXT | 상담 상태 |
| created_at | DATETIME | 생성일 |
| updated_at | DATETIME | 수정일 |

---

# Consultation

| Column | Type |
|---------|------|
| id | INTEGER PK |
| customer_id | FK |
| created_at | DATETIME |
| memo | TEXT |

Relation

Customer 1 : N Consultation

---

# Attachment

| Column | Type |
|---------|------|
| id | INTEGER PK |
| customer_id | FK |
| filename | TEXT |
| filepath | TEXT |
| uploaded_at | DATETIME |

Customer 1 : N Attachment

---

# Index

Customer.phone

Customer.name

Customer.status