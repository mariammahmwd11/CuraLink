# 🏥 CuraLink — Healthcare Management Platform

CuraLink is a full-stack healthcare management platform designed to streamline interactions between **patients, doctors, clinic receptionists, and administrators**.

The platform provides appointment scheduling, online payments, prescription management, medical document management, real-time communication, notifications, clinic management, and AI-assisted drug information.

## 🎥 Demo & Documentation

* **Project Repository:** https://github.com/mariammahmwd11/CuraLink
* **Project Demo Video:** https://1drv.ms/v/c/d63829ad954fd62e/IQDspqB-N4ikRpFU0hRu4QZoAUq7t3LWM3FPuOqeUpSxwYw?e=Awk7tt

---

## ✨ Key Features

### 👤 Patient

* Register and manage personal profile.
* Search for doctors.
* View doctor availability and available appointment slots.
* Book and cancel appointments.
* Complete appointment payments through Stripe.
* Upload, view, download, and delete medical documents.
* View prescriptions and download prescription PDFs.
* Real-time chat with doctors.
* Receive real-time notifications.
* Use the AI-powered Drug Assistant.

### 👨‍⚕️ Doctor

* Manage personal profile.
* Manage clinics and consultation prices.
* Configure weekly availability and appointment slot duration.
* View assigned patients.
* Access authorized patient medical documents.
* Create and manage prescriptions.
* Communicate with patients through real-time chat.
* Receive notifications.

### 👩‍💼 Clinic Receptionist

* Access the assigned clinic dashboard.
* Search for patients.
* Create patient accounts.
* Book appointments on behalf of patients.
* View today's appointments.
* Check patients in.
* View available appointment slots.
* Assist with clinic appointment management.

### 🛡️ Administrator

* Review pending doctor registrations.
* Review uploaded doctor verification documents.
* Approve doctor accounts.
* Manage administrative operations.

---

## 🏗️ Architecture

CuraLink follows a **Clean Architecture** approach with clear separation of concerns between the presentation, application, domain, and infrastructure layers.

```text
                    ┌──────────────────────┐
                    │      MVC Frontend    │
                    └──────────┬───────────┘
                               │ HTTP
                               ▼
                    ┌──────────────────────┐
                    │      API Layer       │
                    │  Minimal APIs / API  │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │   Application Layer  │
                    │ MediatR / CQRS        │
                    │ Validation / UseCases │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │     Domain Layer     │
                    │ Entities / Business  │
                    │ Rules / Abstractions │
                    └──────────┬───────────┘
                               │
                               ▼
                    ┌──────────────────────┐
                    │ Infrastructure Layer │
                    │ EF Core / Identity   │
                    │ External Services    │
                    └──────────┬───────────┘
                               │
                               ▼
                         SQL Database
```

---

## 🛠️ Tech Stack

### Backend

* C#
* .NET 10
* ASP.NET Core
* Minimal APIs
* Entity Framework Core
* LINQ
* MediatR
* CQRS
* FluentValidation
* ASP.NET Core Identity
* JWT Authentication
* SignalR
* Hangfire

### Frontend

* ASP.NET Core MVC
* Razor Views
* JavaScript
* AJAX
* HTML / CSS

### Database

* SQL Server
* Entity Framework Core

### External Services

* Stripe — online payments
* Cloudinary — profile images and medical documents
* Brevo — email communication
* Google Gemini — AI-assisted drug information
* OpenFDA — medication information
* Web Push — browser notifications

### Development & Testing

* Git & GitHub
* Swagger / OpenAPI
* Bruno
* Manual API Testing
* Unit Testing

---

## 🔐 Authentication & Authorization

CuraLink uses **ASP.NET Core Identity** combined with **JWT-based authentication** and role-based authorization.

Supported roles:

```text
Admin
Doctor
Patient
Receptionist
```

Authorization is applied to protect role-specific API endpoints and clinic operations.

### Demo Accounts

The following demo accounts can be used to explore the different user roles in CuraLink:

| Role         | Email                            | Password      |
| ------------ | -------------------------------- | ------------- |
| Admin        | `admin@curalink.com`             | `Admin@12345` |
| Doctor       | `mariammahmoudwork123@gmail.com` | `Doctor123#`  |
| Patient      | `AhmedGamal@gmail.com`           | `Part123#`    |
| Receptionist | `mariiiiiiiiiiio8@gmail.com`     | `Test@123456` |

> These credentials are provided for demonstration and testing purposes only.

---

## 📅 Appointment & Payment Flow

The appointment system supports doctor availability, appointment slot generation, booking, and payment processing.

```text
Patient
   │
   ▼
Select Doctor
   │
   ▼
View Available Slots
   │
   ▼
Book Appointment
   │
   ▼
Create Stripe Checkout Session
   │
   ▼
Complete Payment
   │
   ▼
Stripe Webhook
   │
   ▼
Confirm Payment / Appointment
```

Stripe webhooks are used to process payment status updates.

---

## 💬 Real-Time Communication

CuraLink uses **SignalR** to provide real-time communication between patients and doctors.

The platform supports:

* Real-time chat messages
* Message history
* Message read status
* Real-time notifications

---

## 🔔 Notifications

The notification system supports:

* In-app notifications
* Real-time notifications through SignalR
* Web Push subscriptions
* Unread notification count
* Mark notification as read
* Mark all notifications as read
* Background processing using Hangfire

---

## 🤖 AI Drug Assistant

CuraLink includes an AI-assisted Drug Assistant that combines **Google Gemini** with **OpenFDA medication data**.

The assistant provides information such as:

* Medication name
* Active ingredients
* Dosage information
* Common side effects
* Interaction warnings
* Safety warnings

> ⚠️ The Drug Assistant is intended for informational purposes only and does not replace professional medical advice.

---

## 📄 Medical Documents

Patients can securely manage their medical documents through the platform.

Supported operations include:

* Upload documents
* View documents
* Download documents
* Delete documents

Doctors can access authorized patient medical documents when required for the healthcare workflow.

---

## 🏥 Clinic Management

Doctors can:

* Create clinics
* Update clinic information
* Delete clinics
* Set consultation prices
* Configure availability
* Define appointment slot duration

Each clinic can have assigned receptionists who can manage appointment-related operations.

---

## 📸 Screenshots

### Swagger API Documentation

<img width="3160" height="1775" alt="Screenshot 2026-10-07 223615" src="https://github.com/user-attachments/assets/e281df59-570c-4431-bf81-f1c675bbf070" />
<img width="2815" height="1555" alt="Screenshot 2026-10-07 223632" src="https://github.com/user-attachments/assets/920b8ecc-5dbe-4814-84aa-e01d6eab0c82" />
<img width="3030" height="1780" alt="Screenshot 2026-10-07 223649" src="https://github.com/user-attachments/assets/bd4022de-e9aa-4df5-9687-3383e4469249" />
<img width="2730" height="725" alt="Screenshot 2026-10-07 223700" src="https://github.com/user-attachments/assets/60394f39-0619-42c5-8113-1770b6fdd261" />
<img width="3407" height="1785" alt="Screenshot 2026-10-07 223556" src="https://github.com/user-attachments/assets/91544796-5777-4458-9661-672637488ef4" />


### Patient Dashboard

<img width="3837" height="2045" alt="Screenshot 2026-10-07 230923" src="https://github.com/user-attachments/assets/0e52b5a9-ec4b-4969-b4e3-4812c9d50ff7" />

### Doctor Dashboard

<img width="3835" height="2020" alt="Screenshot 2026-10-07 230938" src="https://github.com/user-attachments/assets/47106f1e-b9cd-426a-aa30-0aba4c2a1642" />

### Receptionist Dashboard

<img width="3837" height="2015" alt="Screenshot 2026-10-07 230956" src="https://github.com/user-attachments/assets/651bf023-873e-44be-acda-617ad76ff08e" />

### Appointment Booking

<img width="3785" height="1825" alt="Screenshot 2026-10-07 231100" src="https://github.com/user-attachments/assets/fe57b6ee-623e-4014-b394-07990d9f2b5c" />

### Stripe Payment

<img width="3795" height="1820" alt="Screenshot 2026-10-07 231149" src="https://github.com/user-attachments/assets/da0f7b94-cf40-4ee6-8674-7c1102437424" />

### Prescription Management

<img width="3822" height="2015" alt="Screenshot 2026-10-07 231034" src="https://github.com/user-attachments/assets/ec124dc9-1cf5-4ea1-bb9e-8a5ab793bf17" />

---

## 🚀 Getting Started

### Prerequisites

Make sure you have:

* .NET 10 SDK
* SQL Server
* Git

External services require their respective API credentials.

### 1. Clone the repository

```bash
git clone https://github.com/mariammahmwd11/CuraLink.git
cd CuraLink
```

### 2. Configure application settings

Configure the required connection strings and external service credentials using **User Secrets** or environment variables.

> Never commit sensitive credentials to the repository.

### 3. Apply database migrations

```bash
dotnet ef database update
```

### 4. Run the API

```bash
dotnet run --project src/CuraLink.API
```

### 5. Run the MVC application

```bash
dotnet run --project src/CuraLink.MVC
```

### 6. Open Swagger

When running in Development mode, Swagger UI is available through the API application.

---

## 🧪 Testing

Critical application workflows were tested through API and manual testing, including:

* Authentication and authorization
* Appointment booking
* Doctor availability
* Prescription operations
* Payment workflow
* Role-based access
* Clinic receptionist operations

API endpoints can be tested using **Swagger** and **Bruno**.

---

## 📂 Project Structure

```text
CuraLink/
│
├── src/
│   ├── CuraLink.API/
│   ├── CuraLink.Application/
│   ├── CuraLink.Domain/
│   ├── CuraLink.Infrastructure/
│   └── CuraLink.MVC/
│
├── tests/
│
├── README.md
└── .gitignore
```

---

## 🔮 Future Improvements

Potential future improvements include:

* Automated integration and end-to-end testing
* CI/CD pipeline
* Advanced analytics and reporting
* Expanded telemedicine capabilities
* Improved AI-assisted healthcare features
* Microservices-based deployment for larger-scale environments

---

## 👩‍💻 Author

**Mariam Mahmoud**

Full-Stack .NET Developer

Built with **ASP.NET Core, .NET 10, Clean Architecture, CQRS, and modern web technologies.**
