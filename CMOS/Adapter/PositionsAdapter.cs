using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using CMOS.Data_Models;

namespace CMOS.Adapter
{
    class PositionsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<PositionsAdapterClickEventArgs> ItemClick;
        public event EventHandler<PositionsAdapterClickEventArgs> ItemLongClick;
        List<Position> Items;

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
            holder.shortName.Text = Items[position].ShortName;
            holder.norm.Text = Items[position].Norm;
            holder.rate.Text = Items[position].Rate;
            holder.order.Text = Items[position].Order;
            holder.color.Text = Items[position].Color;
            holder.id.Text = Items[position].Id.ToString();
        }

        public override int ItemCount => Items.Count;

        void OnClick(PositionsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
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

        public PositionsAdapterViewHolder(View itemView, Action<PositionsAdapterClickEventArgs> clickListener, Action<PositionsAdapterClickEventArgs> longClickListener) : base(itemView)
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
            itemView.Click += (sender, e) => clickListener(new PositionsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new PositionsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class PositionsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}