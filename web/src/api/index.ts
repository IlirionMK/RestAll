import { AuthService } from './auth.service';
import { KitchenService } from './kitchen.service';
import { OrdersService } from './orders.service';
import { ReservationsService } from './reservations.service';
import { TablesService } from './tables.service';
import { RestaurantsService } from './restaurants.service';
import { MenuService } from './menu.service';
import { AnalyticsService } from './analytics.service';
import { LogsService } from './logs.service';
import { StaffService } from './staff.service';
import { UserService } from './user.service';

export const API = {
    auth: AuthService,
    kitchen: KitchenService,
    orders: OrdersService,
    reservations: ReservationsService,
    tables: TablesService,
    restaurants: RestaurantsService,
    menu: MenuService,
    analytics: AnalyticsService,
    logs: LogsService,
    staff: StaffService,
    user: UserService,
};