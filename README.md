# Blazor Chat App

A real-time chat application built with **Blazor Server**, **SignalR**, and **EF Core**. 
This application provides a Teams-like user experience with features for group conversations, online status tracking, multimedia sharing, and message management.

## 🚀 Features

### Core Messaging
- **Real-time Messaging**: Instant message delivery using SignalR.
- **Message Management**:
    - **Edit Messages**: Inline editing for fixing typos or updating content.
    - **Delete Messages**: Remove messages with a secure, custom-styled confirmation modal.
- **File Sharing**: 
    - Upload and share images/files directly in the chat.
    - Visual previews for image attachments.

### Conversations & Presence
- **Conversation Grouping**: 
    - Messages are automatically grouped into conversations (1-on-1 or Groups).
    - Sidebar lists recent chats, sorted by activity.
    - "General (Everyone)" broadcast channel for all users.
- **User Presence**: 
    - Real-time **Online/Offline** status indicators.
    - Status dots shown in the sidebar and chat details.

### User Experience
- **Notifications**:
    - **New Chat Alert**: Popup notification when someone starts a new conversation with you.
    - **Toast Notifications**: In-app popups for incoming messages when viewing a different chat.
    - Auto-dismissing alerts for non-intrusive updates.
- **UI & Design**:
    - **Windows 11 Inspired**: Dark mode layout with Mica-like aesthetics.
    - **Responsive**: Adaptive layout for sidebar and main chat area.

### Identity & Security
- **Authentication**: Built on ASP.NET Core Identity.
- **User Management**: Secure login and registration.
- **Secure Handling**: Chat ownership checks for edit/delete actions (UI enforced).

### Admin Features
- **Admin Dashboard**: Secure administrative views protected by ASP.NET Authorization Roles.
- **Role Management**: Control user access levels and group assignments from an intuitive interface.
- **Reporting & Activity**: 
    - At-a-glance metrics for total message counts and last-active dates.
    - Drill-down message history for any user, enabling auditing without exposing sensitive message content.

## 🛠️ Technologies

- **Framework**: .NET 10 / Blazor Server
- **Real-time**: SignalR
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Styling**: Bootstrap 5 + Custom CSS (Mica Theme)

## 📦 Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) installed.

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/SingularityResearch/ChatApp.git
   cd ChatApp/ChatApp
   ```

2. **Database Setup**
   The app uses SQL Server. Update the `DefaultConnection` string in `appsettings.json` to point to your instance, then apply the migrations to create the database:
   ```bash
   dotnet ef database update
   ```

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Access the App**
   Open your browser to `http://localhost:5000` or `https://localhost:5001` (or the port shown in your terminal).

## 🧩 Project Structure

- **Components/Pages/Chat.razor**: The core chat interface handling UI, logic, and events.
- **Components/Admin/**: Secure administrative views, role management, and detailed activity reports.
- **Hubs/ChatHub.cs**: SignalR Hub managing real-time connections, presence, and message distribution.
- **Services/ChatMessageService.cs**: Data access service for efficiently reading and writing chat payloads to SQL Server.
- **Services/ChatStateService.cs**: Manages in-memory state tracking to observe which users are currently online.
- **Data/**: EF Core context, models, and migrations.

## 🤝 Contributing

1. Fork the Project
2. Create your Feature Branch
3. Commit your Changes
4. Push to the Branch
5. Open a Pull Request
