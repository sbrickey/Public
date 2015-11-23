/**  WPF CONTROL  **/
namespace SBrickey.Libraries.WPF.Controls
{
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Using the Pdfium library, renders to native WPF controls.
    /// </summary>
    /// <remarks>
    /// When the PDF is loaded, its pages are converted to in-memory images, which are
    /// then rendered to WPF as IMAGEs within a SCROLLVIEWER.
    /// </remarks>
    /// <see cref="https://github.com/pvginkel/PdfiumViewer" />
    public partial class ucPdfiumDocumentViewer : ScrollViewer
    {
        private readonly StackPanel PageContainer = new StackPanel() { Background = System.Windows.Media.Brushes.DarkGray };

        // ctor
        public ucPdfiumDocumentViewer()
        {
            // Events : THIS
            this.Loaded += this_Loaded;
            this.Unloaded += this_Unloaded;

            // Events : CONTROLS
            this.PageContainer.MouseEnter += PageContainer_MouseEnter;
            this.PageContainer.PreviewMouseDown += PageContainer_PreviewMouseDown;
            this.PageContainer.PreviewMouseMove += PageContainer_PreviewMouseMove;
            this.PageContainer.PreviewMouseUp += PageContainer_PreviewMouseUp;
        }


        private void this_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.Hand;

            // these settings provide a bit better visual definition for "pages"
            this.PageContainer.Effect = new System.Windows.Media.Effects.DropShadowEffect()
            {
                ShadowDepth = 0,
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 5
            };

            this.Content = PageContainer;
        }
        private void this_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Pdfium_Dispose();
        }


        #region Mouse / Drag tracking/scrolling

        private System.Windows.Point? MouseDragStartingPosition = null;

        private void PageContainer_MouseEnter(object objPageContainer, MouseEventArgs eMouseEnter)
        {
            if (eMouseEnter.LeftButton == System.Windows.Input.MouseButtonState.Released)
                this.MouseDragStartingPosition = null;
        }

        private void PageContainer_PreviewMouseDown(object objPageContainer, MouseButtonEventArgs eMouseDown)
        {
            if (eMouseDown.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;

            // position should be relative to THIS (the scroll viewer), since SETTING the scroll is 
            // relative to THIS offsets
            var OffsetRelativePosition = eMouseDown.GetPosition(this);
            MouseDragStartingPosition = new System.Windows.Point(
               x: this.ContentHorizontalOffset + OffsetRelativePosition.X,
               y: this.ContentVerticalOffset + OffsetRelativePosition.Y
            );
        }
        private void PageContainer_PreviewMouseMove(object objPageContainer, MouseEventArgs eMouseMove)
        {
            if (MouseDragStartingPosition == null)
                return;

            // GetPosition returns relative to THIS (scroll viewer), since SETTING the scroll is
            // relative to THIS offsets
            var OffsetRelativePosition = eMouseMove.GetPosition(this);

            // this calculates the difference from the starting position (relative to the content)
            var displacement = new System.Windows.Vector(
                x: this.ContentHorizontalOffset + OffsetRelativePosition.X - MouseDragStartingPosition.Value.X,
                y: this.ContentVerticalOffset + OffsetRelativePosition.Y - MouseDragStartingPosition.Value.Y
            );
            this.ScrollToVerticalOffset(this.VerticalOffset - displacement.Y);
            this.ScrollToHorizontalOffset(this.VerticalOffset - displacement.X);
        }
        private void PageContainer_PreviewMouseUp(object objPageContainer, MouseButtonEventArgs eMouseUp)
        {
            if (eMouseUp.LeftButton != System.Windows.Input.MouseButtonState.Released)
                return;

            MouseDragStartingPosition = null;
        }

        #endregion

    } // class
} // namespace

/**  PDFIUM  **/
namespace SBrickey.Libraries.WPF.Controls
{
    using PdfiumViewer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    public partial class ucPdfiumDocumentViewer
    {
        private PdfDocument pdfDoc;
        private CancellationTokenSource tokenSource;
        private readonly SynchronizationContext UIThread = SynchronizationContext.Current;
        private readonly Dictionary<int, System.Windows.Controls.Image> pages = new Dictionary<int, System.Windows.Controls.Image>();

        public void Load(string file)
        {
            LoadPDF(() => PdfDocument.Load(file));
        }
        public void Load(byte[] fileContents)
        {
            this.LoadPDF(() =>
            {
                using (var ms = new System.IO.MemoryStream(fileContents))
                    return PdfDocument.Load(ms);
            });
        }
        public void Load(System.IO.Stream stream)
        {
            LoadPDF(() => PdfDocument.Load(stream));
        }


        private void LoadPDF(Func<PdfDocument> LoadDelegate)
        {
            try
            {
                // cancel any outstanding requests
                if (this.tokenSource != null)
                    this.tokenSource.Cancel();

                // redefine a new (non-cancelled) token
                this.tokenSource = new CancellationTokenSource();

                // clear any previously loaded pages
                if (this.PageContainer.Children.Cast<System.Windows.UIElement>().Any() ||
                    this.pages.Any())
                {
                    this.PageContainer.Children.Clear();
                    this.pages.Clear();
                }

                // load the PDF/file
                this.pdfDoc = LoadDelegate();

                // create image controls as placeholders
                for (int page = 1; page <= pdfDoc.PageCount; page++)
                {
                    // generate image control
                    var imgPage = new System.Windows.Controls.Image()
                    {
                        Name = String.Format("Page_{0}", page),
                        Visibility = System.Windows.Visibility.Hidden // hidden until all pages have rendered
                    };

                    // add to local dictionary
                    this.pages.Add(page, imgPage);

                    // and to the on-page container (stackpanel), via a border control (better visual identification of pages)
                    var imgBorder = new System.Windows.Controls.Border()
                    {
                        BorderThickness = new System.Windows.Thickness(1),
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        Margin = new System.Windows.Thickness(8),
                        Child = imgPage
                    };
                    this.PageContainer.Children.Add(imgBorder);
                }

                // in parallel / async, load the individual pages
                Enumerable.Range(1, pdfDoc.PageCount)
                          .AsParallel()
                          .ForAll(i => System.Threading.Tasks.Task.Run(new Action(() => { LoadPage(i); }), tokenSource.Token));
            }
            catch
            {
                tokenSource.Cancel();
                throw;
            }
        } // LoadPDF(...)
        private void LoadPage(int pageNumber)
        {
            tokenSource.Token.ThrowIfCancellationRequested();

            // render the image to the largest screen's resolution.
            // this is important, since the PageSize dimensions are too small (image is blurry)
            //   yet the larger the rendered image, the more memory WPF uses to display.
            // using the display's max width (since the PDF is always scaled to the width of the document)
            //   provides a crisp looking image, while maintaining a smaller memory footprint
            var RenderWidth = System.Windows.Forms.Screen.AllScreens.Max(s => s.Bounds.Width);
            var ScreenToPageSizeRatio = RenderWidth / pdfDoc.PageSizes[pageNumber - 1].Width;
            var RenderHeight = Convert.ToInt32(pdfDoc.PageSizes[pageNumber - 1].Height * ScreenToPageSizeRatio);

            using (var img = pdfDoc.Render(
                                page: pageNumber - 1,
                                width: RenderWidth,
                                height: RenderHeight,
                                dpiX: 96.0f,
                                dpiY: 96.0f,
                                flags: PdfRenderFlags.None
                            ))
            {
                // push the result into the UI thread
                this.UIThread.Send(
                    Marshalled =>
                    {
                        pages[Marshalled.PageNumber].Source = Marshalled.Image;

                        if (pages.All(p => p.Value.Source != null))
                        {
                            pages.ForEach(p => p.Value.Visibility = System.Windows.Visibility.Visible);

                            pdfDoc.Dispose(); // remove any file locks
                        }
                    },
                    new { PageNumber = pageNumber, Image = img.CloneAsBitmapSource() } // clone since original img will be disposed
                );
            } // using img

            GC.Collect();
            return;
        }

        public void Pdfium_Dispose()
        {
            if (tokenSource != null)
                tokenSource.Cancel();

            if (pdfDoc != null)
                pdfDoc.Dispose();

            // dereference / GC the images, since leaving them around can have a HUGE cumulative impact on memory
            this.PageContainer.Children.Clear(); // remove them from the UI tree
            this.pages.ForEach(p => p.Value.Source = null); // dereference the contol's source (BitmapImage)
            this.pages.Clear(); // dereference the control from the dictionary

            GC.Collect();
        }
    }
    
} // namespace

/**  Extension Methods  **/
namespace SBrickey.Libraries.WPF.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class IEnumerableExtensionMethods
    {
        public static void ForEach<T>(this IEnumerable<T> obj, Action<T> act)
        {
            obj.ToList().ForEach(i => act(i));
        }
    }
}
namespace SBrickey.Libraries.WPF.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Media.Imaging;
    public static class ImageExtensionMethods
    {
        public static BitmapSource CloneAsBitmapSource(this Image image) { return CloneAsBitmapSource(image as Bitmap); }

        /// <summary>
        /// Convert an IImage to a WPF BitmapSource. The result can be used in the Set Property of Image.Source
        /// </summary>
        /// <param name="bitmap">The Source Bitmap</param>
        /// <returns>The equivalent BitmapSource</returns>
        public static BitmapSource CloneAsBitmapSource(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            using (var source = (System.Drawing.Bitmap)bitmap.Clone())
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                Interop.GDI.DeleteObject(ptr); //release the HBitmap
                bs.Freeze();
                return bs;
            }
        }
    } // class
}
namespace SBrickey.Libraries.WPF.Controls
{

    using System.Threading;

    /// <summary>
    /// Provides typed Post/Send delegate execution on a specific synchronization context
    /// </summary>
    public static class ThreadingExtensionMethods
    {
        public delegate void SendOrPostCallback<T>(T inp);

        /// <summary>
        /// Asynchronous execution of a delegate, on the context's thread
        /// </summary>
        public static void Post<T>(this SynchronizationContext ctx, SendOrPostCallback<T> delegateCode, T typedObj)
        {
            ctx.Post(
                d: (objParam) => { delegateCode((T)objParam); },
                state: typedObj
            );
        }
        /// <summary>
        /// Synchronous execution of a delegate, on the context's thread
        /// </summary>
        public static void Send<T>(this SynchronizationContext ctx, SendOrPostCallback<T> delegateCode, T typedObj)
        {
            ctx.Send(
                d: (objParam) => { delegateCode((T)objParam); },
                state: typedObj
            );
        }

    } // class
}