using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EasyPDF.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using EasyPDF.ViewModels;
using System.Threading;

namespace EasyPDF.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		public IActionResult ThankYou()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpPost("UploadFiles")]
		public async Task<IActionResult> Post(List<IFormFile> files)
		{
			long size = files.Sum(f => f.Length);
			int i = 1;
			// full path to file in temp location
			List<ConverterPDF> pDFs = new List<ConverterPDF>();

			if (files.Count > 0)
			{
				

				foreach (var formFile in files)
				{
					ConverterPDF converterPDF = await ConvertPDF(formFile);

					converterPDF.Postion = i;

					if (converterPDF == null)
					{

					}
					else
					{
						pDFs.Add(converterPDF);
					}
					i++;
				}
			}

			// process uploaded files
			// Don't rely on or trust the FileName property without validation.

			return View("Converted", pDFs);
		}

		public IActionResult Converted(List<ConverterPDF> pDFs)
		{
			return View(pDFs);
		}

		public async Task<ConverterPDF> ConvertPDF(IFormFile file)
		{
			int intJPEGToPDF;
			int intTIFFToPDF;
			int intImageToPDF;
			

			// Instantiate Object
			APToolkitNET.Toolkit oTK = new APToolkitNET.Toolkit();

			var filePath = Path.GetTempFileName();

			if (file.Length > 0)
			{
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}
			}
			else
			{
				ErrorHandler("Empty File", 0);
				return null;
			}

			ConverterPDF converterPDF = new ConverterPDF();

			// Toolkit can directly convert an image to a PDF file
			var pdfPath = Path.GetTempFileName();

			switch (file.ContentType)
			{
				case "image/jpeg":
					// If the image is a JPEG file use JPEGToPDF
					intJPEGToPDF = oTK.JPEGToPDF(filePath, pdfPath);
					if (intJPEGToPDF < 1)
					{
						ErrorHandler("JPEGToPDF", intJPEGToPDF);
						return null;
					}
					break;
				case "image/tiff":
					// If the image file is a TIFF file use TIFFToPDF
					intTIFFToPDF = oTK.TIFFToPDF(filePath, pdfPath);
					if (intTIFFToPDF != 1)
					{
						ErrorHandler("TIFFToPDF", intTIFFToPDF);
						return null;
					}
					break;
				default:
					// Any supported image file can be converted to PDF with ImageToPDF
					intImageToPDF = oTK.ImageToPDF(filePath, pdfPath);
					if (intImageToPDF != 1)
					{
						ErrorHandler("ImageToPDF", intImageToPDF);
						return null;
					}
					break;
			}

			Thread.Sleep(50);
			
			int intOpenInputFile;
			int intOpenOutputFile;
			int intCopyForm;
			var stampedPath = Path.GetTempFileName();

			intOpenOutputFile = oTK.OpenOutputFile(stampedPath);
			if (intOpenOutputFile != 0)
			{
				ErrorHandler("OpenOutputFile", intOpenOutputFile);
			}

			intOpenInputFile = oTK.OpenInputFile(pdfPath);
			if (intOpenInputFile != 0)
			{
				ErrorHandler("OpenInputFile", intOpenInputFile);
			}

			oTK.SetFont("Helvetica", 12, -1);
			string strTitle = "Signature:";
			float textWidth = oTK.GetTextWidth(strTitle);
			oTK.PrintText(10, 10, strTitle, -1);

			// Copy the template (with the stamping changes) to the new file
			// Start page and end page, 0 = all pages
			intCopyForm = oTK.CopyForm(0, 0);
			if (intCopyForm != 1)
			{
				ErrorHandler("CopyForm", intCopyForm);
			}

			oTK.CloseOutputFile();

			// Release Object
			oTK.Dispose();


			converterPDF.JPGFileLocation = filePath;
			converterPDF.PDFFileLocation = stampedPath;

			return converterPDF;
			// Process Complete
			//WriteResults("Done!");
		}

		[HttpPost]
		public async Task<ActionResult> GetPDFData(string pdf)
		{
			return Content(Convert.ToBase64String(System.IO.File.ReadAllBytes(pdf)));
		}

		// Error Handling
		public static void ErrorHandler(string strMethod, object rtnCode)
		{
			//WriteResults(strMethod + " error:  " + rtnCode.ToString());
		}

	}
}
