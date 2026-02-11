using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CutHalf
{
    internal class CutHalfLogic
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="isTextSelection">true if we are selecting text, false if we are just navigating</param>
        /// <param name="calculateNextCaretColumnPositionFunc">arguments: currentLineTotalColumnsCount, currentCaretColumn</param>
        internal static void CutHalf(AsyncPackage package, IAsyncServiceProvider serviceProvider, bool isTextSelection, Func<int, int, int> calculateNextCaretColumnPositionFunc)
        {
            IVsTextManager textManager = package.GetServiceAsync(typeof(SVsTextManager)).Result as IVsTextManager;

            textManager.GetActiveView(1, null, out IVsTextView textViewAdapter);

            var componentModel = serviceProvider.GetServiceAsync(typeof(SComponentModel)).Result as IComponentModel;
            var editorAdapterFactory = componentModel.GetService<IVsEditorAdaptersFactoryService>();
            IWpfTextView wpfTextView = editorAdapterFactory.GetWpfTextView(textViewAdapter);

            if (wpfTextView != null)
            {
                int currentCaretLine = 0, currentCaretColumn = 0;
                // Получаем позицию каретки
                textViewAdapter.GetCaretPos(out currentCaretLine, out currentCaretColumn);

                IVsTextLines textLines = null;
                textViewAdapter.GetBuffer(out textLines);
                int currentLineTotalColumnsCount = 0;
                textLines.GetLengthOfLine(currentCaretLine, out currentLineTotalColumnsCount);

                int nextCaretColumnPosition = calculateNextCaretColumnPositionFunc(currentLineTotalColumnsCount, currentCaretColumn);

                if (isTextSelection)
                {
                    textViewAdapter.GetSelection(out int piAnchorLine, out int piAnchorCol, out int piEndLine, out int piEndCol);

                    textViewAdapter.SetSelection(currentCaretLine, piAnchorCol, currentCaretLine, nextCaretColumnPosition);
                }
                else
                {
                    textViewAdapter.SetCaretPos(currentCaretLine, nextCaretColumnPosition);
                }
            }
        }
    }
}
