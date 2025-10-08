# Imgeneus Server Setup Guide

Welcome to **Imgeneus**!
This guide explains how to set up and run the project locally, including the game servers and database.

---

## 🧩 Prerequisites

Make sure you have the following installed:

* [Visual Studio 2022](https://visualstudio.microsoft.com/) with .NET SDK
* [Git](https://git-scm.com/)
* [MySQL 8.0.22](https://dev.mysql.com/downloads/mysql/)
* (Optional) [Git Bash](https://gitforwindows.org/) for running commands

---

## ⚙️ Installation Steps

### 1. Clone the repository and update submodules

```bash
git clone https://github.com/yourusername/Imgeneus.git
cd Imgeneus
git submodule update --init --recursive
```

### 2. Build the solution

Open the solution in **Visual Studio**, then **Build → Build Solution**.

All projects should build successfully.
You should see:

```
========== Build: 15 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========
```

---

### 3. Install and configure MySQL

1. Install **MySQL 8.0.22**.
2. Remember the password for your `root` user — you’ll need it later.

---

### 4. Update configuration files

Locate all `appsettings.json` files in the following projects:

* `Imgeneus.Login`
* `Imgeneus.World`
* `Imgeneus.Database`
* `Imgeneus.Authentication`

Replace all instances of `your_password` with your MySQL root password.

---

### 5. Run database migrations

You can apply migrations in one of two ways:

**Using console:**

```bash
dotnet ef database update
```

**Using Visual Studio Package Manager Console:**

```powershell
Update-Database
```

---

### 6. Populate the database

1. Open `setup.bat`, located in:
   `src/Imgeneus.Database/Migrations/sql`
2. Replace `your_password` with your MySQL root password.
3. Run the `.bat` file to insert required data.

---

### 7. Configure and start the servers

In Visual Studio:

1. Set both **Login** and **World** projects as startup projects.
2. In the build configuration dropdown, select **SHAIYA_US_DEBUG**.
3. Run the solution.

If successful, you’ll have two running servers:

* **Login Server:** port `30800`
* **World Server:** port `30810`

Admin panels will be available at:

* `http://localhost:5000`
* `http://localhost:5001`

When you register the first user, it will automatically become the **admin**.

---

### 8. Run the game client

Download the client from:
[https://archive.openshaiya.org/api/build/shaiya-ga-ps0032.tar.gz](https://www.elitepvpers.com/link/?https://archive.openshaiya.org/api/build/shaiya-ga-ps0032.tar.gz)

Start the game from the console:

```bash
game.exe start 127.0.0.1 user_name:password
```

---

## 🧠 Troubleshooting

* If a build fails, ensure all submodules are initialized and restored.
* Verify that MySQL is running and the credentials in `appsettings.json` are correct.
* Ports `30800`, `30810`, `5000`, and `5001` must be available (not used by other applications).

---

## 💬 Contact

For questions or support, reach out via:

* **Discord:** `annamelashkina`
* **Microsoft Teams:** [Join link](https://teams.live.com/l/invite/FEAtcvev0hNl12ryQE?v=g1)
* **Email:** anna.osiatnik@gmail.com
