# NeonNet

A locally hosted desktop social network built with C# Windows Forms. Users can create accounts, post content under topics, message each other, follow users and topics, and receive a personalized feed powered by a scoring algorithm.

---

## Features

- **Account System** — Sign up with email or phone number, log in with credentials, and agree to terms and conditions before registering
- **Home Feed** — Personalized feed powered by a scoring algorithm that ranks posts based on follows, interactions, topic preferences, and recency
- **Explore Page** — Browse trending topics, suggested users, and suggested posts
- **Create Post** — Write and publish posts under searchable topics, with followed topics appearing as priority suggestions
- **Create Topic** — Create new topic categories for the platform
- **Messages** — Send and receive direct messages with other users
- **Notifications** — Receive notifications when someone follows you or sends you a message
- **Profile Page** — View post count, followers, following count, pronouns, and profile picture. Follow or message other users directly from their profile
- **Comments** — Comment on posts, with the ability to edit and delete your own comments
- **Follow Users** — Follow and unfollow other users with live count updates
- **Follow Topics** — Follow topic categories to boost related posts in your feed
- **Settings** — Update pronouns and profile photo from the settings menu
- **Moderation** — Built-in content moderation that flags inappropriate posts and topics

---

## Tech Stack

- **Language:** C#
- **Framework:** Windows Forms (.NET)
- **Database:** SQLite (local, no server required)
- **IDE:** Visual Studio 2022

---

## Requirements

- Windows 10 or Windows 11
- .NET Framework installed
- Visual Studio 2022 (to build and run from source)

---

## Getting Started

1. Clone or download the repository
2. Open `NeonNet3.sln` in Visual Studio
3. Build the solution using **Ctrl + Shift + B**
4. Run the application using **F5** or the Start button
5. The SQLite database (`NeonNet.db`) will be created automatically on first run
6. Create an account and log in to get started

---

## Project Structure

NeonNet3/
├── Data/
│   └── Database.cs          — All database operations
├── Models/                  — Data models
├── Services/
│   └── ModerationService.cs — Content moderation logic
├── CommentControl.cs        — Comment UI component
├── CreatePostScreen.cs      — Create post screen
├── CreateTopicScreen.cs     — Create topic screen
├── DashboardScreen.cs       — Main dashboard
├── ExploreScreen.cs         — Explore page
├── FeedScreen.cs            — Feed display
├── HomeScreen.cs            — Home screen wrapper
├── LoginScreen.cs           — Login screen
├── MessagesScreen.cs        — Messaging screen
├── NotificationsScreen.cs   — Notifications screen
├── PostViewScreen.cs        — Post detail and comments
├── ProfileScreen.cs         — User profile screen
├── SearchResultsScreen.cs   — Search results
├── SignupScreen.cs          — Signup screen
└── NeonNet.db               — Local SQLite database
****
---

## Version History

| Version | Key Updates |
|---|---|
| V1 | Account validation — password, username, and email rules |
| V2 | Messages page UI |
| V3 | Fully functional messaging with database |
| V4 | Email input fix, messages layout fix, notifications screen |
| V5 | Profile screen — settings, pronouns, followers, following, sorting tabs, custom scrollbar, follow users, message from profile, follow topics |
| V6 | Editable and deletable comments, searchable topic field, terms and conditions on signup, topic-based algorithm scoring |

---

## Authors

Ernesto — Messaging system, notifications screen, profile screen, database layer, comments system, topic search, terms and conditions, algorithm updates

---

## Notes

- All data is stored locally. No internet connection is required
- Profile images are saved in a `ProfileImages` folder inside the application directory
- Passwords are stored as plain text in the current version — hashing is planned for a future update

