# Project: Console Library Manager

**Audience:** Intern with zero C# experience
**Duration:** 1–2 days
**Type:** Console application (.NET, no UI, no database)

---

## Goal

Build a small console app that manages a library: add items, list them, borrow and return them.
By building it, the intern learns the core C# and OOP basics hands-on.

## What You Will Learn

| Concept | Where it shows up in the project |
|---|---|
| Variables, types, `string`, `int`, `bool` | Properties like `Title`, `Year`, `IsBorrowed` |
| `if` / `switch` / loops | The console menu |
| **Classes & objects** | `Book`, `Dvd`, `Member`, `Library` |
| **Properties & constructors** | Creating items with data |
| **Encapsulation** | `private` fields, public methods |
| **Inheritance** | `Book` and `Dvd` inherit from `LibraryItem` |
| **Abstract classes** | `LibraryItem` is never created directly |
| **Polymorphism** | `Describe()` behaves differently per item type |
| **Interfaces** | `IBorrowable` |
| `List<T>` | Storing items and members |
| Basic LINQ (optional, light) | Searching by title |

---

## Requirements

### Classes

```
IBorrowable (interface)
 ├─ bool Borrow(Member member)
 └─ void Return()

LibraryItem (abstract class) : IBorrowable
 ├─ int Id
 ├─ string Title
 ├─ int Year
 ├─ bool IsBorrowed
 ├─ Member? BorrowedBy
 ├─ abstract string Describe()
 ├─ Borrow(...)   -> fails if already borrowed
 └─ Return()

Book : LibraryItem
 ├─ string Author
 ├─ int Pages
 └─ Describe()  -> "Book: Title by Author (Year), 320 pages"

Dvd : LibraryItem
 ├─ int DurationMinutes
 └─ Describe()  -> "DVD: Title (Year), 120 min"

Member
 ├─ int Id
 ├─ string Name
 └─ List<LibraryItem> BorrowedItems

Library
 ├─ List<LibraryItem> Items
 ├─ List<Member> Members
 ├─ AddItem(...)
 ├─ AddMember(...)
 ├─ ListItems()
 ├─ FindByTitle(string)
 ├─ BorrowItem(itemId, memberId)
 └─ ReturnItem(itemId)

Program (Main)
 └─ Console menu loop
```

### Menu

```
=== Library Manager ===
1. Add book
2. Add DVD
3. Add member
4. List all items
5. Search item by title
6. Borrow item
7. Return item
0. Exit
```

### Rules

- An item that is already borrowed cannot be borrowed again.
- A member can borrow max. **3** items at once.
- Invalid input (wrong ID, empty title, text instead of number) must show a friendly message and **not crash** the app.

---

## Suggested Plan

### Day 1: Foundations (≈ 4–6 h)

1. **Setup** – Create the project: `dotnet new console -n LibraryManager`, run "Hello World".
2. **Mini C# crash course** – Variables, `if`, loops, `Console.ReadLine()`. Build a tiny menu that just prints what was chosen.
3. **First class** – Create `Member` with properties and a constructor. Create two members in `Main` and print them.
4. **Inheritance** – Create abstract `LibraryItem`, then `Book` and `Dvd`. Put both in a `List<LibraryItem>` and loop over it calling `Describe()`.
5. **✅ Checkpoint:** Items and members can be created and listed from the menu.

### Day 2: Behavior & Polish (≈ 4–6 h)

1. **Interface** – Add `IBorrowable`, implement it in `LibraryItem`.
2. **Library class** – Move the lists and logic out of `Program` into `Library`.
3. **Borrow / Return** – Implement the rules above.
4. **Search** – `FindByTitle` (case-insensitive, partial match).
5. **Input validation** – Use `int.TryParse` and null/empty checks.
6. **Cleanup & demo** – Seed some sample data, present the app to the mentor.

---

## Definition of Done

- [ ] App builds and runs without errors
- [ ] 3 menu options work
- [ ] At least 1 abstract class, 1 interface, 2 derived classes
- [ ] Borrow rules are enforced
- [ ] App does not crash on bad input (time based)
- [ ] Intern can explain **why** `LibraryItem` is abstract and what the interface is for

---

## Mentor Notes

**Review questions to ask at the end:**

1. What is the difference between a class and an object?
2. Why use `private` fields / properties instead of making everything public?
3. What is the difference between an abstract class and an interface?
4. What happens when you call `Describe()` on a `LibraryItem` variable that holds a `Dvd`? Why?
5. Why is the logic in `Library` and not in `Program`?

**Tips:**

- Let him write the code himself, and only step in when he's stuck for >15 min.
- Do a short code review at the end of each day.
