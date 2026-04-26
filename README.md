# Meeting Room Reservation System

This project is a complex full-stack solution for managing and reserving meeting rooms. I built the entire system to cover the needs of both regular users (via the web) and administrators (via a desktop application), with both components communicating through a shared API.

## 🛠 Technologies and Architecture

When designing the system, I focused on a clean separation of layers and maximum performance:
* **Backend:** ASP.NET Core Web API. I chose **Dapper** as the ORM because it gives me full control over SQL queries and is extremely fast [cite: MujProjekt.txt].
* **Database:** SQL Server (LocalDB). The design includes relationships, constraints, and indexes to ensure data integrity [cite: MujProjekt.txt].
* **Web Frontend:** ASP.NET Core MVC using Razor Pages. For interactivity, I implemented **FullCalendar.js**, and for visualization, I used **Chart.js** [cite: MujProjekt.txt].
* **Desktop:** WPF (Windows Presentation Foundation) with asynchronous communication via HttpClient [cite: MujProjekt.txt].
* **Security:** Authentication via **JWT tokens** (for the API) and **Cookies** (for the web). Passwords are hashed using **BCrypt.Net** [cite: MujProjekt.txt].

## 🌐 Web Application (User Perspective)

The goal was to create an intuitive interface where users can quickly find a vacant room:
* **Smart Calendar:** I integrated FullCalendar so users can see occupancy visually. I added a feature allowing users to create a reservation simply by dragging the mouse over the calendar [cite: MujProjekt.txt].
* **Live Profile:** In the profile section, users see their reservations divided into scheduled, ongoing, and history. Using **AJAX** (Fetch API), a user can cancel a future reservation without reloading the page, with the row dynamically moving to the history table [cite: MujProjekt.txt].
* **Statistical Dashboard:** I created room utilization charts directly on the web. It combines a bar chart (number of reservations) and a line chart (total time) for a selected period [cite: MujProjekt.txt].
* **Filtering:** Users can filter rooms not only by time but also by minimum capacity – this processing happens directly at the database level for maximum efficiency [cite: MujProjekt.txt].

## 🖥 Desktop Application (Administration)

The WPF application is used for comprehensive system management:
* **Entity Management:** The administrator can manage users, rooms, and reservations in organized grids with support for batch operations [cite: MujProjekt.txt].
* **Draft System:** In the reservation dialog, I implemented a draft queue. The admin can prepare several reservations at once in a side panel and only save them to the database (either all at once or individually) after review [cite: MujProjekt.txt].
* **Real-time Validation:** When creating a reservation, the system immediately suggests the room capacity and enforces time limits [cite: MujProjekt.txt].

## 🧠 Advanced Logic and Security Features

I paid close attention to details that make the application robust:
* **Status History:** Every change to a reservation (creation, cancellation, activation) is automatically recorded in the `ReservationHistory` table [cite: MujProjekt.txt]. I ensured that even when a user is deleted, a transaction runs to cancel their future reservations and record it in the history [cite: MujProjekt.txt].
* **Referential Integrity:** Reservation deletion is performed within a **SqlTransaction**. The dependent history is cleared first, followed by the reservation itself, to prevent database foreign key errors [cite: MujProjekt.txt].
* **Data Inference Protection:** I modified the SQL queries so that when searching by "Purpose", the system always returns foreign reservations as "Occupied," even if they match the search text [cite: MujProjekt.txt]. This prevents an attacker from "mining" the content of private reservations.
* **Collision Resolution:** Both the backend and frontend ensure that overlapping reservations in the same room are not created [cite: MujProjekt.txt]. At the API level, I return a `409 Conflict`, which both the desktop and web clients can handle [cite: MujProjekt.txt].
* **API Documentation:** Using **reflection**, I created the `/api/Docs` endpoint, which traverses controllers in real-time and generates a JSON description of all methods, parameters, and return types, fulfilling the project requirements [cite: MujProjekt.txt, Zadání projektu - 16-02-2026.pdf].

## 📋 Fulfillment of Assignment Requirements

The application fully meets the requirements for Theme 2 [cite: Zadání projektu - 16-02-2026.pdf]:
1. **Registration/Login:** Completed (Web and API Auth) [cite: MujProjekt.txt].
2. **Grids with DB-level sorting:** Completed (All overviews use ORDER BY in SQL) [cite: MujProjekt.txt].
3. **Profile page with AJAX:** Completed (Reservation cancellation without reload) [cite: MujProjekt.txt].
4. **At least 2 non-trivial forms with validation:** Completed (Reservations with time/capacity checks, Room management with dynamic equipment) [cite: MujProjekt.txt].
5. **Endpoint with API description via reflection:** Completed (`DocsController`) [cite: MujProjekt.txt].
6. **Status history with timestamp:** Completed (`ReservationHistory`) [cite: MujProjekt.txt].
7. **Utilization statistics:** Completed (Interactive Chart.js graphs) [cite: MujProjekt.txt].
8. **Specific checks:** Completed (Collisions, prohibition of editing past reservations, maximum length checks) [cite: MujProjekt.txt].

---
*Created as a semester project for the course Programming in C# II.*
