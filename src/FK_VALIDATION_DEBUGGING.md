# FK Validation Debugging Guide

I've added **debug logging** to trace exactly where the FK validation is happening.

## Steps to Debug:

### 1. **Restart Backend API** (`Apha.FPS.Api`)
   - Stop if running
   - Start in **DEBUG mode** (F5)
   - Open **Output window**: `View → Output → Show output from: Debug`

### 2. **Restart Frontend Web** (`Apha.FPSApps.Web`)
   - Stop if running
   - Start (F5 or Ctrl+F5)
   - Navigate to Division Maintenance

### 3. **Test Adding "EU Exit"**
   - Click "Add Division"
   - Fill form:
     - Division ID: `999`
     - Agency ID: `(select any)`
     - Division Name: `EU Exit`
     - Central Overhead: `0`
   - Click **Save**

### 4. **Check Backend API Console/Output Window**

You should see logs like:
```
[DivisionRepository] Checking FK references for divName: 'EU Exit'
[DivisionRepository] ProfitCentre check result: True
[DivisionRepository] DivisionGrade check result: True
[DivisionRepository] Total FK references found: 2

[DivisionService] Checking FK references for: EU Exit
[DivisionService] FK references found: 2 - Tables: tblkpprofitcentre, divisiongrade
[DivisionService] THROWING FK VALIDATION ERROR: Cannot add Division Name as it is already used in tblkpprofitcentre, divisiongrade
```

### 5. **Check Browser Developer Tools** (F12)

#### Console Tab:
Look for:
```javascript
saveDivision called
Saving division data: {...}
Error saving: (xhr object)
```

#### Network Tab:
- Find request: `/FPS/DivisionMaintenance/Create`
- **Status Code**: Should be `400 Bad Request`
- **Response tab**: Check JSON format

Expected Response:
```json
{
  "success": false,
  "message": "Cannot add Division Name as it is already used in tblkpprofitcentre, divisiongrade",
  "errors": [
    {
      "code": "BUSINESS_LOGIC_ERROR",
      "message": "Cannot add Division Name as it is already used in tblkpprofitcentre, divisiongrade"
    }
  ]
}
```

## What to Provide:

Please copy and paste:

### A) Backend Console Output
All lines starting with `[DivisionRepository]` and `[DivisionService]`

### B) Browser Console Logs  
All logs when you click Save

### C) Network Tab - Status Code
What HTTP status code do you see?

### D) Network Tab - Response JSON
Copy the entire response from the Response tab

### E) UI Behavior
What message/error do you actually see on the screen?

---

## Common Issues:

### If NO backend logs appear:
- Code is not being executed
- Wrong division name being sent
- Request not reaching backend

### If logs show "FK references found: 0":
- Data doesn't exist in database
- Wrong table/column names
- Query filter issue

### If Status Code is 200:
- Exception not being thrown
- Backend returning success incorrectly

### If Status Code is 400 but no error displays:
- Frontend error handler not working
- Response format mismatch
- JavaScript error

---

This debug info will pinpoint the exact issue! 🎯
