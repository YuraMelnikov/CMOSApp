using System;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using CMOS.Data_Models;
using Android.Graphics;

namespace CMOS.Adapter
{
    class PositionsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<PositionsAdapterClickEventArgs> ItemLongClick;
        List<Position> Items;
        public event EventHandler<int> ItemClick;

        public PositionsAdapter(List<Position> Data)
        {
            Items = Data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.orderPositions, parent, false);
            var vh = new PositionsAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as PositionsAdapterViewHolder;
            holder.name.Text = Items[position].Name;
            holder.code.Text = Items[position].Code;
            holder.weight.Text = Items[position].Weight;

            if (holder.weight.Text == "0")
            {
                holder.weight.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#910000")));
                var typeface = Typeface.Create("<FONT FAMILY NAME>", Android.Graphics.TypefaceStyle.Bold);
                holder.weight.Typeface = typeface;
            }
            holder.shortName.Text = Items[position].ShortName;
            holder.norm.Text = Items[position].Norm;
            holder.rate.Text = Items[position].Rate;
            if (holder.norm.Text == holder.rate.Text)
            {
                holder.cardItem.SetBackgroundColor(Android.Graphics.Color.ParseColor("#9ED9CC"));
            }
            else if (holder.rate.Text != "0")
            {
                holder.cardItem.SetBackgroundColor(Android.Graphics.Color.ParseColor("#F3DB74"));
            }
            else
            {
            }
            holder.order.Text = Items[position].Order;
            holder.color.Text = Items[position].Color;
            if (holder.color.Text == "RAL1021")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#EEC900")));
            else if (holder.color.Text == "RAL3005")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#5E2028")));
            else if (holder.color.Text == "RAL3020")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#C1121C")));
            else if (holder.color.Text == "RAL3031")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#AC323B")));
            else if (holder.color.Text == "RAL5003")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#2A3756")));
            else if (holder.color.Text == "RAL5012")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#3481B8")));
            else if (holder.color.Text == "RAL6002")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#276235")));
            else if (holder.color.Text == "RAL6019")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#B7D9B1")));
            else if (holder.color.Text == "RAL7001")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#8c969d")));
            else if (holder.color.Text == "RAL7035")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#CBD0CC")));
            else if (holder.color.Text == "RAL7036")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#9A9697")));
            else if (holder.color.Text == "RAL ")
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#ffffff")));
            else
                holder.color.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.ParseColor("#000000")));
            holder.id.Text = Items[position].Id.ToString();
        }

        public override int ItemCount => Items.Count;

        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }
        void OnLongClick(PositionsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class PositionsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView name { get; set; }
        public TextView code { get; set; }
        public TextView weight { get; set; }
        public TextView shortName { get; set; }
        public TextView norm { get; set; }
        public TextView rate { get; set; }
        public TextView order { get; set; }
        public TextView color { get; set; }
        public TextView id { get; set; }
        public CardView cardItem { get; set; }

        public PositionsAdapterViewHolder(View itemView, Action<int> clickListener, Action<PositionsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            name = (TextView)itemView.FindViewById(Resource.Id.name);
            code = (TextView)itemView.FindViewById(Resource.Id.code);
            weight = (TextView)itemView.FindViewById(Resource.Id.weight);
            shortName = (TextView)itemView.FindViewById(Resource.Id.shortName);
            norm = (TextView)itemView.FindViewById(Resource.Id.norm);
            rate = (TextView)itemView.FindViewById(Resource.Id.rate);
            order = (TextView)itemView.FindViewById(Resource.Id.order);
            color = (TextView)itemView.FindViewById(Resource.Id.color);
            id = (TextView)itemView.FindViewById(Resource.Id.id);
            cardItem = (CardView)itemView.FindViewById(Resource.Id.cardItem);
            itemView.Click += (sender, e) => clickListener(base.LayoutPosition);
            itemView.LongClick += (sender, e) => longClickListener(new PositionsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class PositionsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}