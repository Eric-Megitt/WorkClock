using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        string spreadsheetId = "1XX4iy6M6mFYEznlGBf7oP9Qf7TVLhv-jomoBxRC2dLg";
        string range = "Sheet1!A1"; // Specify the cell where you want to add the comment

        GoogleCredential credential;
        using (var stream = new System.IO.FileStream("C:\\Users\\Sneric\\RiderProjects\\WorkClock\\WorkClock\\credentials.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream)
                .CreateScoped(new[] { SheetsService.Scope.Spreadsheets });
        }

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Google Sheets API Comment Adder"
        });

        var request = new Request()
        {
            UpdateCells = new UpdateCellsRequest()
            {
                Fields = "note",
                Range = new GridRange
                {
                    SheetId = 0,
                    StartRowIndex = 0,
                    EndRowIndex = 1,
                    StartColumnIndex = 0,
                    EndColumnIndex = 1
                },
                Rows = new List<RowData>
                {
                    new RowData
                    {
                        Values = new List<CellData>
                        {
                            new CellData
                            {
                                Note = "This is a comment for the cell."
                            }
                        }
                    }
                }
            }
        };

        BatchUpdateSpreadsheetRequest batchUpdateRequest = new BatchUpdateSpreadsheetRequest
        {
            Requests = new List<Request> { request }
        };

        // Execute the batch update request
        service.Spreadsheets.BatchUpdate(batchUpdateRequest, spreadsheetId).Execute();

        Console.WriteLine("Comment added successfully.");
    }
}
