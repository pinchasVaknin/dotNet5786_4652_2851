# dotNet5786_4652_2851

# ?? Project Bonuses and Features List

For the convenience of the reviewer, below is a table detailing the bonuses implemented in the project and their location in the code.

---

## ?? System Users

Use the following details to log into the system:

### ??? Admin Login
* **Username:** `333333333`
* **Password:** `1`

### ?? Courier Login
* **Username:** (Any courier ID from the system after initialization)
* **Password:** (The password created for them - can be seen in XML)

---

## ??? Data and Logic Layers (DAL & BL)

| Topic / Bonus | Status | Code Location (Click to Open) | Lines | Notes | Points |
| :--- | :---: | :---: | :--- | :---: | :---: |
| **Input Validation (TryParse)** | ? | [`DalTest/Program.cs`](./DalTest/Program.cs) | 449, 470, 478, 486, 494 | Full use of `int.TryParse`, `double.TryParse`, `DateTime.TryParse`, `Enum.TryParse` with return value checking and inline variable declaration | 1 |
| **Singleton & Thread Safe (DalList)** | ? | [`DalList/DalList.cs`](./DallList/DalList.cs) | 15-22 | Using `Lazy<T>` with `LazyThreadSafetyMode` for thread-safe Singleton with Lazy Initialization | 2 |
| **Singleton & Thread Safe (DalXml)** | ? | [`DalXml/DalXml.cs`](./DalXml/DalXml.cs) | 15-22 | Using `Lazy<T>` with `LazyThreadSafetyMode` for thread-safe Singleton with Lazy Initialization | - |
| **Password Property** | ? | [`DalFacade/DO/Courier.cs`](./DalFacade/DO/Courier.cs) | 7 | `CourierPassword` property in the Courier entity | 2 |
| **Initial Password by Admin** | ? | [`DalTest/Initialization.cs`](./DalTest/Initialization.cs) | 85 | Creating initial passwords for couriers during initialization | 1 |
| **Password Update by Courier** | ? | [`BL/BlImplementation/CourierImplementation.cs`](./BL/BlImplementation/CourierImplementation.cs) | 73-78 | Courier can update their password via `UpdateCourier` | - |
| **Distance Calculation by Vehicle Type** | ? | [`BL/Helpers/Tools.cs`](./BL/Helpers/Tools.cs) | 295-340 | `GetActualDistanceAsync` uses OSRM API with profiles: car, bike, foot | 3 |

---

## ?? User Interface (WPF / UI)

| Topic / Bonus | Status | Code Location (Click to Open) | Lines | Notes | Points |
| :--- | :---: | :---: | :--- | :---: | :---: |
| **Interactive Graphical Display for Invalid Input** | ? | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 68-72 | TextBlocks with error messages in red that appear/disappear | 1 |
| **Validation Integrated with Binding** | ? | [`PL/Styles/TextBoxes.xaml`](./PL/Styles/TextBoxes.xaml) | 13-22 | Triggers for appearance change based on `IsReadOnly`; Converters for enum validation | 1 |
| **Icon in Title and Taskbar** | ? | [`PL/Helpers/Image.ico`](./PL/Helpers/Image.ico) / [`LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 17 | Custom application icon | 1 |
| **Property Trigger** | ? | [`PL/Styles/Buttons.xaml`](./PL/Styles/Buttons.xaml) | 23-30, 48-51 | `IsMouseOver`, `IsPressed` triggers for Opacity and Foreground changes | 1 |
| **Data Trigger** | ? | [`PL/Styles/DataGrids.xaml`](./PL/Styles/DataGrids.xaml) | 10-14 | `EnumDataGridCellStyle` with Converter for coloring by Enum | 1 |
| **ControlTemplate** | ? | [`PL/Styles/Buttons.xaml`](./PL/Styles/Buttons.xaml) | 11-31 | Custom Template with Border, ContentPresenter and Triggers | 1 |
| **ENTER Key as Button Click** | ? | [`PL/Login/LoginWindow.xaml.cs`](./PL/Login/LoginWindow.xaml.cs) | 74-88 | `HandleEnterKey` handles `Key.Enter` to trigger Login or focus navigation | 1 |
| **Password Hidden (Asterisks)** | ? | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 64 | Using `PasswordBox` to hide input | 1 |
| **Smart Delete Button (Courier)** | ? | [`PL/Converters.cs`](./PL/Converters.cs) | 90-115 | `ConvertDeleteToEnabled` checks `IsCourierDeletable()` to control button | 2 |
| **Smart Cancel Button (Order)** | ? | [`PL/Converters.cs`](./PL/Converters.cs) | 117-135 | `ConvertCancelToEnabled` checks `OrderStatus` (Open/InProgress) | - |

---

## ?? Simulator

| Topic / Bonus | Status | Code Location (Click to Open) | Lines | Notes | Points |
| :--- | :---: | :---: | :--- | :---: | :---: |
| **Block Operations During Simulator (BL)** | ? | [`BL/Helpers/AdminManager.cs`](./BL/Helpers/AdminManager.cs) | 288-293 | `ThrowOnSimulatorIsRunning()` throws `BlTemporaryNotAvailableException` | - |
| **Exception Catching in PL** | ? | [`PL/MainWindow.xaml.cs`](./PL/MainWindow.xaml.cs) | All Button handlers | All buttons wrapped in try-catch with MessageBox | - |
| **Thread-Safe Observer Pattern** | ? | [`PL/Helpers/ObserverMutex.cs`](./PL/Helpers/ObserverMutex.cs) | Entire file | Mutex pattern to prevent race conditions in UI updates | - |

---

## ?? Points Summary

| Category | Points |
|----------|--------|
| Development Environment (TryParse) | **1** |
| DAL Layer (Singleton + Password) | **4** |
| BL Layer (Password + Distance) | **4** |
| PL Layer - Advanced WPF | **10** |
| **Total** | **19** |

---

## ??? System Architecture

### System Layers:
1. **DAL (Data Access Layer)** - Data storage in XML and lists with Singleton Pattern
2. **BL (Business Logic)** - Business rules, validation and simulator
3. **PL (Presentation Layer)** - WPF interface with MVVM patterns

### Key Features:
- **Multi-threaded Simulator** - Automatic order assignment and delivery completion
- **Observer Pattern** - Real-time UI updates across all windows
- **Thread-Safe Operations** - Mutex-based protection for concurrent access
- **External API Integration** - Nominatim (geocoding) and OSRM (routing)

---

## ?? Quick Start

1. Open `dotNet5786_4652_2851.sln` in Visual Studio 2022
2. Set `PL` as the startup project
3. Build and run the solution
4. Click "Initialize Database" to create initial data
5. Log in with admin credentials:
   - **ID:** `333333333`
   - **Password:** `1`

---

## ?? Project Structure

```
dotNet5786_4652_2851/
??? DalFacade/     # DAL interfaces and data objects (DO)
??? DallList/ # In-memory DAL implementation
??? DalXml/      # XML-based DAL implementation
??? DalTest/     # DAL testing console application
??? BL/        # Business Logic layer
??? BlTest/ # BL testing console application
??? PL/          # WPF Presentation Layer
?   ??? Controls/       # Custom user controls
?   ??? Converters.cs   # Value Converters
?   ??? Helpers/ # Utility classes
?   ??? Styles/         # Resource Dictionaries
?   ??? Courier/        # Courier management windows
?   ??? Order/          # Order management windows
?   ??? Login/          # Authentication windows
?   ??? delivery/       # Delivery history windows
??? Stage0/       # Stage 0 project
```

---

## ?? Developers

- Student ID: 4652
- Student ID: 2851