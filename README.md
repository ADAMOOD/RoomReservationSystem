# Meeting Room Reservation System

A comprehensive, full-stack solution for managing and reserving meeting rooms, featuring a web application for regular users and a robust desktop application for administrators.

## 🚀 Project Overview

The system is designed to streamline the process of reserving meeting rooms while enforcing business rules such as room capacity limits, maximum reservation durations, and time-conflict prevention. It follows a multi-tier architecture using modern .NET technologies.

## 🏗️ Architecture & Technologies

- **Backend:** ASP.NET Core Web API with Dapper ORM for high-performance database access.
- **Web Frontend:** ASP.NET Core MVC for regular user interactions.
- **Desktop Frontend:** WPF (Windows Presentation Foundation) for administrative tasks.
- **Database:** Relational Database Management System (SQL Server).
- **Communication:** Secure asynchronous JSON-based API communication with token authentication.

## 🌐 Web Application Features (User Side)

- **Identity Management:** User registration and secure login system.
- **Room Discovery:** Advanced room overview with filtering by time availability and capacity.
- **Interactive User Profile:**
    - Real-time updates of upcoming and ongoing reservations using **AJAX**.
    - Visual progress indicators for active sessions.
- **Robust Validation:** All forms implement strict server-side validation to ensure data integrity.
- **API Documentation:** Auto-generated JSON documentation of the API interface using **Reflection**.

## 🖥️ Desktop Application Features (Admin Side)

- **Admin Dashboard:** Centralized management of users and all system entities.
- **Smart Reservation Management:**
    - **Omnibox Search:** Intelligent filtering that automatically distinguishes between IDs and names for rooms and users.
    - **Live Filtering:** Instant UI updates as the administrator types.
    - **Batch Operations:** Efficient deletion and management of multiple records simultaneously.
- **Advanced Dialog System:**
    - **Draft Queue:** Administrators can prepare multiple reservations or rooms in a side panel (Drafts) before committing changes to the database.
    - **Visual Hints:** Real-time information about room capacity and time limits displayed during reservation creation.
- **UI/UX Enhancements:**
    - Integration of **Extended WPF Toolkit** for professional `DateTimePicker` components.
    - Precision time handling (automatic cleaning of seconds to synchronize with web timers).
    - Custom-templated modern UI components with hover and click effects.

## 🛡️ Business Logic & Rules

- **Conflict Prevention:** The system strictly prevents overlapping active reservations for the same room.
- **Historical Integrity:** Past or ongoing reservations are locked; they cannot be edited or deleted to preserve the history of changes.
- **Time Constraints:** Enforced maximum reservation minutes per room and prevention of reservations set in the past.
- **History Tracking:** All state objects maintain a detailed history of changes with timestamps.
