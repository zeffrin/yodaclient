using System;
using System.Collections;
using System.Windows.Forms;

namespace Etier.IconHelper
{
	/// <summary>
	/// Maintains a list of currently added file extensions
	/// </summary>
	public class IconListManager
	{
		private Hashtable _extensionList = new Hashtable();
		private System.Collections.ArrayList _imageLists = new ArrayList();			//will hold ImageList objects
		private IconHelper.IconReader.IconSize _iconSize;
		bool ManageBothSizes = false; //flag, used to determine whether to create two ImageLists.

        public enum IconTypes { FolderClosed, FolderOpen, Default, Up };

        /// <summary>
		/// Creates an instance of <c>IconListManager</c> that will add icons to a single <c>ImageList</c> using the
		/// specified <c>IconSize</c>.
		/// </summary>
		/// <param name="imageList"><c>ImageList</c> to add icons to.</param>
		/// <param name="iconSize">Size to use (either 32 or 16 pixels).</param>
		public IconListManager(System.Windows.Forms.ImageList imageList, IconReader.IconSize iconSize )
		{
			// Initialise the members of the class that will hold the image list we're
			// targeting, as well as the icon size (32 or 16)
            _imageLists.Add( imageList );
			_iconSize = iconSize;
            InitializeImageList(imageList, iconSize);
		}
		
		/// <summary>
		/// Creates an instance of IconListManager that will add icons to two <c>ImageList</c> types. The two
		/// image lists are intended to be one for large icons, and the other for small icons.
		/// </summary>
		/// <param name="smallImageList">The <c>ImageList</c> that will hold small icons.</param>
		/// <param name="largeImageList">The <c>ImageList</c> that will hold large icons.</param>
		public IconListManager(System.Windows.Forms.ImageList smallImageList, System.Windows.Forms.ImageList largeImageList )
		{
			//add both our image lists
			_imageLists.Add( smallImageList );
			_imageLists.Add( largeImageList );

            InitializeImageList(smallImageList, IconReader.IconSize.Small);
            InitializeImageList(largeImageList, IconReader.IconSize.Large);

			//set flag
			ManageBothSizes = true;
		}

        private void InitializeImageList(ImageList imagelist, IconReader.IconSize iconSize)
        {
            imagelist.Images.Add(IconReader.GetFolderIcon(iconSize, IconReader.FolderType.Closed));
            imagelist.Images.Add(IconReader.GetFolderIcon(iconSize, IconReader.FolderType.Open));
            imagelist.Images.Add(IconReader.GetFileIcon(".", iconSize, false));
            imagelist.Images.Add(YodaClient.Properties.Resources.up);
        }
        
        /// <summary>
		/// Used internally, adds the extension to the hashtable, so that its value can then be returned.
		/// </summary>
		/// <param name="Extension"><c>String</c> of the file's extension.</param>
		/// <param name="ImageListPosition">Position of the extension in the <c>ImageList</c>.</param>
		private void AddExtension( string Extension, int ImageListPosition )
		{
			_extensionList.Add( Extension, ImageListPosition );
		}

		/// <summary>
		/// Called publicly to add a file's icon to the ImageList.
		/// </summary>
		/// <param name="filePath">Full path to the file.</param>
		/// <returns>Integer of the icon's position in the ImageList</returns>
        public int GetIcon( string fileName )
		{
            // Split it down so we can get the extension
			//string[] splitPath = fileName.Split(new Char[] {'.'});
            string extension = null;
            int i;
            //extension = (string)splitPath.GetValue( splitPath.GetUpperBound(0) );

            for (i = fileName.Length - 1; i > 0; i--)
            {
                // if we find the extension delimter then return the characters found so far
                if (fileName[i] == '.')
                {
                    // check that the extension delimiter is not the last thing, if it is return
                    // default icon for no extension
                    if (i == fileName.Length)
                        return 2;

                    extension = fileName.Remove(i);
                    break;
                }
                // otherwise if we hit the path delimiter return the default icon for no extension
                if (fileName[i] == '/' || fileName[i] == '\\')
                    return 2;

            }
            // if it makes it through the loop and discovers no delimters then return unknown file icon
            if (i == 0)
            {
                return 2;
            }

			//Check that we haven't already got the extension, if we have, then
			//return back its index
			if (_extensionList.ContainsKey( extension.ToUpper() ))
			{
				return (int)_extensionList[extension.ToUpper()];		//return existing index
			} 
			else 
			{
				// It's not already been added, so add it and record its position.

				int pos = ((ImageList)_imageLists[0]).Images.Count;		//store current count -- new item's index

				if (ManageBothSizes == true)
				{
					//managing two lists, so add it to small first, then large
					((ImageList)_imageLists[0]).Images.Add( IconReader.GetFileIcon( fileName, IconReader.IconSize.Small, false ) );
					((ImageList)_imageLists[1]).Images.Add( IconReader.GetFileIcon( fileName, IconReader.IconSize.Large, false ) );
				} 
				else
				{
					//only doing one size, so use IconSize as specified in _iconSize.
					((ImageList)_imageLists[0]).Images.Add( IconReader.GetFileIcon( fileName, _iconSize, false ) );	//add to image list
				}

				AddExtension( extension.ToUpper(), pos );	// add to hash table
				return pos;
			}
		}

        public int GetIcon(IconTypes iconType)
        {
            return (int)iconType;
        }

        /// <summary>
		/// Clears any <c>ImageLists</c> that <c>IconListManager</c> is managing.
		/// </summary>
		public void ClearLists()
		{
			foreach( ImageList imageList in _imageLists )
			{
				imageList.Images.Clear();	//clear current imagelist.
			}
			
			_extensionList.Clear();			//empty hashtable of entries too.
		}
	}
}
