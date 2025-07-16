# 🎬 MovieTicketMVC – ASP.NET MVC Application

An advanced web application for managing and purchasing cinema tickets, built using the **ASP.NET MVC Framework** in **Visual Studio 2022**. The system offers a full-featured experience including CRUD operations, user and admin authentication, dynamic content filtering, secure ticket booking, and real-time seat reservation.

---

## 📌 Features Overview

### 🏠 **1. Home Page**
- Displays a welcome message upon loading
- Intuitive and responsive navigation bar

### 🎞️ **2. Movie Sections**
#### 📽️ Current Movies
- Lists movies currently showing (initialized via code)
- Displays: title, duration, start date, genre
- Filterable and searchable by title and 18+ status

#### 🕒 Coming Soon
- Lists upcoming movies with future release dates
- Same features as Current Movies
- Admins can **add new movies only in this section**

#### 🔎 Movie Details
- Detailed view includes: full description, 18+ rating, trailer video (locally stored), cast, language, etc.
- Accessible via **Details** button for each movie

#### 🛠️ Admin Controls
- Admins can:
  - **Add**, **Edit**, or **Delete** movies
  - Upload trailers (saved to `Content/Videos`)
  - Automatically move movies between sections based on the date
  - Enforce rating format (0.0–10.0), and required fields validation
- Non-admins can only view movie details

---

## 🎟️ **3. Ticket Purchasing**

> **Note:** Available only for logged-in users.

### 🛒 Purchase Flow
1. **Movie Selection:** Dropdown of current & upcoming movies
2. **Date Selection:**
   - Current movies: from today onward
   - Coming Soon: from release date onward
3. **Time Slot Selection:** Only future times (4 coded options)
4. **Seat Selection:**
   - Visual 10x10 seat grid
   - Rows 1-2: 450 MKD, Rows 3-10: 200 MKD
   - Real-time seat locking
   - Total price updates dynamically
5. **User Validation:**
   - Age check for 18+ films
   - Required valid email (pre-filled but editable)
6. **Confirmation:**
   - Displays booking details: movie, date, time, seats, total, email
   - Sends confirmation email with a **PDF ticket**
   - Seat is marked as reserved (disabled in future)

### 📋 Booking History
- Regular users: view their own booking history
- Admins: view all user purchases
- Option to list all purchased tickets in the system

---

## 📬 **4. Contact Section**
- Displays:
  - Email
  - Phone number
  - Social media links

---

## 🔐 Authentication & Authorization
- Role-based access:
  - **Admin**: Full access to manage movies and bookings
  - **User**: Can view and purchase tickets
- Redirects unauthorized users to login with helpful prompts

---

## 💾 File Management
- Trailer videos are uploaded to `/Content/Videos`
- Old trailers are automatically deleted when replaced or removed
- PDF generation and email confirmation handled on successful ticket booking

---

## 📂 Technologies Used
- ASP.NET MVC (C#)
- Entity Framework
- Razor Views
- Bootstrap & jQuery
- Git for version control
- Visual Studio 2022

---

## 🛠️ Getting Started

### Clone the repository:
```bash
git clone https://github.com/nikolasarafimov/MovieTicketMVCApp.git
