// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function LoadPdf(url, gui) {

    // Initialization settings
    readerplus.initializeSettings({
        protocol: "http",
        hostname: 'localhost',
        port: 62625,
        language: 'en'
    });
    
    readerplus.Document.addEventListener("load", function () {
        // Remove download from orb and main menu
        readerplus.mainmenu.orb.Download.remove();
        readerplus.mainmenu.orb.Save.remove();
        readerplus.mainmenu.orb.Print.remove();
        readerplus.mainmenu.File.General.Download.remove();
        readerplus.mainmenu.File.General.Save.remove();
        readerplus.mainmenu.File.General.Print.remove();

        // Remove page menu
        readerplus.mainmenu.Page.remove();

        // Remove comment section from tool menu
        readerplus.mainmenu.Tools.Comment.remove();
        
        // Add new menu in Tools->Insert
        readerplus.mainmenu.File.General.addItem(
            "PrintAndSave",
            "PrintAndSave",
            "/images/PaS.png",
            false,
            "Print And Save",
            "",
            function () {
                readerplus.Document.save();
                readerplus.Document.print();
            });
    });

    // Document can be saved to any location when it is submitted by a user
    readerplus.Document.addEventListener("submit", function (strResult) {
        var result = JSON.parse(strResult);
        if (result.Status === 0) {
            // Open the submitted document in another tab and redirect to thankyou.html
            var submittedPDFData = result.Details;
            let pdfwindow = window.open("");
            pdfwindow.document.write("<iframe width='100%' height='100%' src='data:application/pdf;base64, " + encodeURI(submittedPDFData) + "'></iframe>");
            window.location = "Home/ThankYou";
        }
        else {
            // Display alert on error
            alert('Document failed to submit!')
            console.error(result.Details);
        }
    });

    // Open document
    $.ajax({
        type: "POST",
        url: "/Home/GetPDFData",
        data: { pdf: url },
        //dataType: "json",
        //contentType: "application/json; charset=utf-8",
        //async: false,
        success: function (data) {
            // Document settings
            var isMasterDocument = 1;
            var editMode = 1;

            // Upload the document into the viewer
            var result = readerplus.Document.upload(data, isMasterDocument, editMode, "", "DocumentName.pdf");
            if (result.Status === 0) {
                // Save document ID in order to reopen a document from the Reader Plus data store
                var docID = readerplus.Document.getDocumentID();
                // Open document in edit mode
                readerplus.Document.edit(docID);
            }
            else {
                // Display alert on error
                alert('Document failed to open!');
                console.error(result.Details);
            }
        },
        error: function (xhr, status, error) {
            alert("Document failed to open!");
        }
    });
}