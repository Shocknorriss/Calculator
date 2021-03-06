﻿using Data.Appsettings;
using Domain.Entites;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace AnnonsonMVC.Utilities
{
    public class ImageService
    {
        private readonly AppSettings _appSettings;

        public ImageService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public string CreateDirectoryForImage(Article newArticle)
        {
            newArticle.ImageFileName = "aid" + newArticle.ArticleId + "-" + Guid.NewGuid();
            string year = DateTime.Now.ToString("yyyy");
            string month = DateTime.Now.ToString("MM");
            string day = DateTime.Now.ToString("dd");

            var todaysDate = year + @"\" + month + @"\" + day;
            var imageDirectoryPath = Path.Combine(_appSettings.MediaFolder + todaysDate).Trim();

            if (!Directory.Exists(imageDirectoryPath))
            {
                Directory.CreateDirectory(imageDirectoryPath);
            }

            return imageDirectoryPath;
        }

        public string SaveImageToDirectory(Article newArticle, string imageDirectoryPath, IFormFile imageFile)
        {
            var saveImagePath = Path.Combine(imageDirectoryPath, newArticle.ImageFileName).Replace(@"\\", @"\");

            using (var imagestream = new FileStream(saveImagePath, FileMode.Create))
            {
                imageFile.CopyTo(imagestream);
            }
            newArticle.ImagePath = DateTime.Now.ToString("yyy-MM-dd").Replace("-", @"\");
            newArticle.ImageFileFormat = "jpg";                          

            return saveImagePath;
        }

        public string CreateResizeImagesToImageDirectory(IFormFile imageFile, string saveImagePath, string imageDirectoryPath)
        {
            Stream imageStream = imageFile.OpenReadStream();
            Image resizeImage = Image.FromStream(imageStream);
            var imageFileBitmapSize = resizeImage.Size;
            var article = new Article();

            if (resizeImage.Width >= 2048)
            {
                CreateNewImageAndFillOutBlankSpaces(2048, 2048, resizeImage, saveImagePath, imageDirectoryPath);
            }
            if (resizeImage.Width >= 1024)
            {
                CreateNewImageAndFillOutBlankSpaces(1024, 1024, resizeImage, saveImagePath, imageDirectoryPath);
                article.ImageWidths += "1024, ";
            }
            if (resizeImage.Width >= 512)
            {
                CreateNewImageAndFillOutBlankSpaces(512, 512, resizeImage, saveImagePath, imageDirectoryPath);
                article.ImageWidths += "512, ";
            }
            if (resizeImage.Width >= 256)
            {
                CreateNewImageAndFillOutBlankSpaces(256, 256, resizeImage, saveImagePath, imageDirectoryPath);
                article.ImageWidths += "256, ";
            }
            if (resizeImage.Width >= 128)
            {
                CreateNewImageAndFillOutBlankSpaces(128, 128, resizeImage, saveImagePath, imageDirectoryPath);
                article.ImageWidths += "128";
            }
            resizeImage.Dispose();
            imageStream.Dispose();
            return article.ImageWidths;
        }

        public Image CreateNewImageAndFillOutBlankSpaces(int canvasWidth, int canvasHeight, Image resizeImage, string saveImagePath, string imageDirectoryPath)
        {
            var originalImage = new Bitmap(saveImagePath);

            Image newImageSize = new Bitmap(canvasWidth, canvasHeight);
            Graphics graphic = Graphics.FromImage(newImageSize);

            graphic.CompositingQuality = CompositingQuality.HighQuality;
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

            double ratioX = (double)canvasWidth / (double)resizeImage.Width;
            double ratioY = (double)canvasHeight / (double)resizeImage.Height;

            double ratio = ratioX < ratioY ? ratioX : ratioY;

            int newHeight = Convert.ToInt32(resizeImage.Height * ratio);
            int newWidth = Convert.ToInt32(resizeImage.Width * ratio);

            int posX = Convert.ToInt32((canvasWidth - (resizeImage.Width * ratio)) / 2);
            int posY = Convert.ToInt32((canvasHeight - (resizeImage.Height * ratio)) / 2);

            graphic.Clear(Color.LightGray);
            graphic.DrawImage(originalImage, posX, posY, newWidth, newHeight);
           
            ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
            EncoderParameters encoderParameters;
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

            using (var fileStream = new FileStream(saveImagePath + "-" + canvasWidth + ".jpg", FileMode.Create))
            {
                newImageSize.Save(fileStream, info[1], encoderParameters);
            }
            originalImage.Dispose();
            return newImageSize;
        }

        public void TryToDeleteOriginalImage(string imagepath)
        {
            if (File.Exists(imagepath))
            {
                File.Delete(imagepath);
            }
            else
            {
               Console.WriteLine("Path doesnt exist");
            }
        }

        public void DeleteArticleImages(string path, string fileName)
        {
            var fullpath = Path.Combine(_appSettings.MediaFolder + path);
            string[] fileList = Directory.GetFiles(fullpath, "*" + fileName + "*");
            foreach (string file in fileList)
            {
                File.Delete(file);
            }
        }        
    }
 }


