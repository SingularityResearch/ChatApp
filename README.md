# Blazor Chat App

A real-time chat application built with **Blazor Server**, **SignalR**, and **EF Core**. 
This application provides a Teams-like user experience with features for group conversations, online status tracking, and file sharing.

## 🚀 Features

- **Real-time Messaging**: Instant message delivery using SignalR.
- **Conversation Grouping**: 
    - Messages are grouped into conversations (1-on-1 or Groups).
    - Sidebar lists recent chats with the most active ones on top.
    - "General (Everyone)" broadcast channel.
- **User Presence**: 
    - Real-time **Online/Offline** status indicators.
    - Status dots shown in the sidebar and chat details.
- **File Sharing**: 
    - Upload and share images/files directly in the chat.
- **Notifications**:
    - **New Chat Alert**: Popup notification when someone starts a new conversation with you.
    - **Toast Notifications**: In-app popup for incoming messages when viewing a different chat.
    - Audio/Visual cues (Toast auto-dismisses after 5s).
- **Identity & Security**: 
    - Built on ASP.NET Core Identity.
    - Secure login and registration.

## 🛠️ Technologies

- **Framework**: .NET / Blazor Server
- **Real-time**: SignalR
- **Database**: SQLite (Development default)
- **ORM**: Entity Framework Core
- **Styling**: Bootstrap 5 + Custom Mica-inspired CSS

## 📦 Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) installed.

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/SingularityResearch/ChatApp.git
   cd ChatApp
   ```

2. **Database Setup**
   The app uses SQLite by default. The database is created automatically on first run via migrations.
   ```bash
   dotnet ef database update
   ```
   *(Optional if `dotnet run` handles it, but good practice)*.

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Access the App**
   Open your browser to `http://localhost:5238` (or the port shown in your terminal).

## 🧩 Project Structure

- **Components/Pages/Chat.razor**: The core chat interface handling UI, logic, and events.
- **Hubs/ChatHub.cs**: SignalR Hub managing real-time connections and presence.
- **Services/ChatStateService.cs**: Manages in-memory state for online users and message broadcasting event streams.
- **Data/**: EF Core context and migrations.

## 🤝 Contributing

1. Fork the Project
2. Create your Feature Branch
3. Commit your Changes
4. Push to the Branch
5. Open a Pull Request
