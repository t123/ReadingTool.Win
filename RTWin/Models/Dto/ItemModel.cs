using System;
using Ninject;
using NPoco;
using RTWin.Core.Enums;
using RTWin.Entities;
using RTWin.Services;

namespace RTWin.Models.Dto
{
    public class ItemModel : BaseDtoModel
    {
        private string _mediaUri;
        private string _l1Title;
        public long ItemId { get; set; }
        public ItemType ItemType { get; set; }
        public string CollectionName { get; set; }
        public int? CollectionNo { get; set; }
        public DateTime? LastRead { get; set; }

        public string MediaUri
        {
            get { return _mediaUri; }
            set { _mediaUri = value; OnPropertyChanged("MediaUri"); }
        }

        public string L1Title
        {
            get { return _l1Title; }
            set { _l1Title = value; OnPropertyChanged("L1Title"); }
        }

        public string L1Content { get; set; }
        public long L1LanguageId { get; set; }
        public string L2Title { get; set; }
        public string L2Content { get; set; }
        public long? L2LanguageId { get; set; }
        public int ReadTimes { get; set; }
        public int ListenedTimes { get; set; }

        public bool IsParallel
        {
            get { return !string.IsNullOrWhiteSpace(L2Content); }
        }

        public bool HasMedia
        {
            get { return !string.IsNullOrWhiteSpace(MediaUri); }
        }

        /// <summary>
        /// This property is not guarenteed to be NOT NULL
        /// </summary>
        [ResultColumn]
        public string L1Language { get; set; }

        /// <summary>
        /// This property is not guarenteed to be NOT NULL
        /// </summary>
        [ResultColumn]
        public string L2Language { get; set; }

        [Ignore]
        public string CommonName
        {
            get
            {
                string name = string.IsNullOrEmpty(CollectionName) ? "" : CollectionName + " - ";
                name += CollectionNo == null ? "" : CollectionNo.ToString() + ". ";
                name += L1Title;

                if (!string.IsNullOrWhiteSpace(L1Language))
                {
                    name += " (" + L1Language + ")";
                }

                return name;
            }
        }

        public Item ToItem()
        {
            var itemService = App.Container.Get<ItemService>();
            var i = itemService.FindOne(this.ItemId);

            if (i == null)
            {
                i = Item.CreateItem();
            }

            i.CollectionName = this.CollectionName;
            i.CollectionNo = this.CollectionNo;
            i.ItemType = this.ItemType;
            i.L1Content = this.L1Content;
            i.L1LanguageId = this.L1LanguageId;
            i.L1Title = this.L1Title;
            i.L2Content = this.L2Content;
            i.L2LanguageId = this.L2LanguageId;
            i.L2Title = this.L2Title;
            i.MediaUri = this.MediaUri;

            return i;
        }
    }
}