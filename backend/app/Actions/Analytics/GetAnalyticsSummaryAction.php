<?php

declare(strict_types=1);

namespace App\Actions\Analytics;

use App\Enums\OrderStatus;
use App\Models\Order;
use App\Models\OrderItem;
use App\Models\Reservation;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\DB;

class GetAnalyticsSummaryAction
{
    public function execute(): array
    {
        return [
            'revenue'      => $this->revenue(),
            'orders'       => $this->orders(),
            'top_items'    => $this->topItems(),
            'reservations' => $this->reservations(),
        ];
    }

    private function revenue(): array
    {
        $paid = fn () => Order::where('status', OrderStatus::PAID);

        return [
            'today'      => (float) $paid()->whereDate('paid_at', Carbon::today())->sum('total_amount'),
            'this_week'  => (float) $paid()->whereBetween('paid_at', [Carbon::now()->startOfWeek(), Carbon::now()->endOfWeek()])->sum('total_amount'),
            'this_month' => (float) $paid()->whereMonth('paid_at', Carbon::now()->month)->whereYear('paid_at', Carbon::now()->year)->sum('total_amount'),
        ];
    }

    private function orders(): array
    {
        $paid = fn () => Order::where('status', OrderStatus::PAID);

        return [
            'today'         => $paid()->whereDate('paid_at', Carbon::today())->count(),
            'this_week'     => $paid()->whereBetween('paid_at', [Carbon::now()->startOfWeek(), Carbon::now()->endOfWeek()])->count(),
            'this_month'    => $paid()->whereMonth('paid_at', Carbon::now()->month)->whereYear('paid_at', Carbon::now()->year)->count(),
            'average_value' => (float) round((float) $paid()->avg('total_amount'), 2),
        ];
    }

    private function topItems(): array
    {
        return OrderItem::select(
            'name',
            DB::raw('SUM(quantity) as quantity_sold'),
            DB::raw('SUM(price * quantity) as revenue')
        )
            ->whereHas('order', fn ($q) => $q->where('status', OrderStatus::PAID))
            ->groupBy('name')
            ->orderByDesc('quantity_sold')
            ->limit(5)
            ->get()
            ->map(fn ($item) => [
                'name'          => $item->name,
                'quantity_sold' => (int) $item->quantity_sold,
                'revenue'       => (float) round($item->revenue, 2),
            ])
            ->toArray();
    }

    private function reservations(): array
    {
        return [
            'today'     => Reservation::whereDate('reservation_time', Carbon::today())->count(),
            'this_week' => Reservation::whereBetween('reservation_time', [Carbon::now()->startOfWeek(), Carbon::now()->endOfWeek()])->count(),
        ];
    }
}
