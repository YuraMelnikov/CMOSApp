using System;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using CMOS.Data_Models;
using System.Collections.Generic;

namespace CMOS.Adapter
{
    class OrdersAdapter : RecyclerView.Adapter
    {
        public event EventHandler<OrdersAdapterClickEventArgs> ItemLongClick;
        List<Order> Items;
        public event EventHandler<int> ItemClick;

        public OrdersAdapter(List<Order> Data)
        {
            Items = Data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ordersList, parent, false);
            var vh = new OrdersAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var holder = viewHolder as OrdersAdapterViewHolder;
            holder.numberTN.Text = Items[position].NumberTN;
            holder.positions.Text = Items[position].PositionName;
            holder.customer.Text = Items[position].Customer;
            holder.orderId.Text = Items[position].Id;
            holder.percent.Text = Items[position].PercentComplited;
        }

        public override int ItemCount => Items.Count;

        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }

        void OnLongClick(OrdersAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }

    public class OrdersAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView numberTN { get; set; }
        public TextView positions { get; set; }
        public TextView customer { get; set; }
        public TextView orderId { get; set; }
        public TextView percent { get; set; }

        public OrdersAdapterViewHolder(View itemView, Action<int> clickListener, Action<OrdersAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            numberTN = (TextView)itemView.FindViewById(Resource.Id.numberTN);
            positions = (TextView)itemView.FindViewById(Resource.Id.positions);
            customer = (TextView)itemView.FindViewById(Resource.Id.customer);
            orderId = (TextView)itemView.FindViewById(Resource.Id.orderId);
            percent = (TextView)itemView.FindViewById(Resource.Id.percent);
            itemView.Click += (sender, e) => clickListener(base.LayoutPosition);
            itemView.LongClick += (sender, e) => longClickListener(new OrdersAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class OrdersAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}