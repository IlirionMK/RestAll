import 'package:flutter/material.dart';
import '../../main.dart';
import '../profile_screen.dart';
import '../reservation_screen.dart';
import 'menu_screen.dart';
import '../../services/api_service.dart';

class TablesScreen extends StatefulWidget {
  const TablesScreen({super.key});
  @override
  State<TablesScreen> createState() => _TablesScreenState();
}

class _TablesScreenState extends State<TablesScreen> {
  final ApiService _apiService = ApiService();
  List<dynamic> _tables = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _isLoading = true);
    final data = await _apiService.getTables();
    if (mounted) {
      setState(() {
        _tables = data;
        _isLoading = false;
      });
    }
  }

  void _changeStatus(int id, String currentStatus) {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (context) => Container(
        padding: const EdgeInsets.symmetric(vertical: 20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text("Zmień status stolika", style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
            const SizedBox(height: 15),
            _statusOption(id, 'free', 'Dostępny', Colors.green),
            _statusOption(id, 'occupied', 'Zajęty', Colors.red),
            _statusOption(id, 'reserved', 'Zarezerwowany', Colors.orange),
            const SizedBox(height: 10),
          ],
        ),
      ),
    );
  }

  Widget _statusOption(int id, String status, String label, Color color) {
    return ListTile(
      leading: CircleAvatar(backgroundColor: color.withAlpha(50), child: Icon(Icons.circle, color: color, size: 16)),
      title: Text(label, style: const TextStyle(fontWeight: FontWeight.w500)),
      onTap: () async {
        Navigator.pop(context);
        final err = await _apiService.updateTableStatus(id, status);
        if (err == null) {
          _load();
        } else {
          if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(err)));
        }
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    String role = (ApiService.userRole ?? 'guest').toLowerCase();
    bool isStaff = role == 'waiter' || role == 'admin' || role == 'manager';

    return Scaffold(
      backgroundColor: Colors.grey[50],
      appBar: AppBar(
        title: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text("Zarządzanie RestAll", style: TextStyle(fontWeight: FontWeight.bold, fontSize: 20)),
            Text(role.toUpperCase(), style: const TextStyle(fontSize: 12, color: Colors.white70)),
          ],
        ),
        backgroundColor: Colors.deepOrange,
        foregroundColor: Colors.white,
        elevation: 0,
        actions: [
          IconButton(icon: const Icon(Icons.refresh), onPressed: _load),
          const SizedBox(width: 5),
        ],
      ),
      drawer: Drawer(
        child: Column(
          children: [
            UserAccountsDrawerHeader(
              accountName: Text(ApiService.currentUser?['name'] ?? "Użytkownik"),
              accountEmail: Text(ApiService.currentUser?['email'] ?? "brak@email.com"),
              currentAccountPicture: const CircleAvatar(backgroundColor: Colors.white, child: Icon(Icons.person, color: Colors.deepOrange, size: 40)),
              decoration: const BoxDecoration(color: Colors.deepOrange),
            ),
            ListTile(
              leading: const Icon(Icons.person_outline),
              title: const Text("Mój Profil"),
              onTap: () {
                Navigator.pop(context);
                Navigator.push(context, MaterialPageRoute(builder: (_) => const ProfileScreen()));
              },
            ),
            ListTile(
              leading: const Icon(Icons.calendar_today_outlined),
              title: const Text("Rezerwacje"),
              onTap: () {
                Navigator.pop(context);
                Navigator.push(context, MaterialPageRoute(builder: (_) => const ReservationScreen()));
              },
            ),
            const Spacer(),
            const Divider(),
            ListTile(
              leading: const Icon(Icons.logout, color: Colors.red),
              title: const Text("Wyloguj", style: TextStyle(color: Colors.red)),
              onTap: () {
                ApiService.token = null;
                Navigator.pushAndRemoveUntil(context, MaterialPageRoute(builder: (_) => const LoginScreen()), (r) => false);
              },
            ),
            const SizedBox(height: 20),
          ],
        ),
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _load,
              child: GridView.builder(
                padding: const EdgeInsets.all(16),
                gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  crossAxisSpacing: 16,
                  mainAxisSpacing: 16,
                  childAspectRatio: 0.9,
                ),
                itemCount: _tables.length,
                itemBuilder: (context, index) {
                  final t = _tables[index];
                  final status = t['status'].toString().toLowerCase();
                  Color color = Colors.green;
                  String displayStatus = "Dostępny";
                  if (status == 'occupied') { color = Colors.red; displayStatus = "Zajęty"; }
                  if (status == 'reserved') { color = Colors.orange; displayStatus = "Zarezerwowany"; }

                  return InkWell(
                    onTap: () => _showTableActions(t),
                    borderRadius: BorderRadius.circular(20),
                    child: Card(
                      elevation: 2,
                      shadowColor: color.withAlpha(80),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
                      child: Container(
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(20),
                          gradient: LinearGradient(
                            begin: Alignment.topLeft,
                            end: Alignment.bottomRight,
                            colors: [Colors.white, color.withAlpha(15)],
                          ),
                        ),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Container(
                              padding: const EdgeInsets.all(12),
                              decoration: BoxDecoration(color: color.withAlpha(25), shape: BoxShape.circle),
                              child: Icon(Icons.table_restaurant, color: color, size: 40),
                            ),
                            const SizedBox(height: 12),
                            Text("Stolik ${t['number']}", style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
                            const SizedBox(height: 4),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                              decoration: BoxDecoration(color: color, borderRadius: BorderRadius.circular(12)),
                              child: Text(displayStatus.toUpperCase(), style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.bold)),
                            ),
                          ],
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
    );
  }

  void _showTableActions(dynamic table) {
    String role = (ApiService.userRole ?? 'guest').toLowerCase();
    bool isStaff = role == 'waiter' || role == 'admin' || role == 'manager';

    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (context) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(vertical: 20),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text("Stolik ${table['number']} - Opcje", style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
              const SizedBox(height: 20),
              if (isStaff)
                ListTile(
                  leading: const Icon(Icons.edit_note, color: Colors.blue),
                  title: const Text("Zmień status stolika"),
                  subtitle: const Text("Ręczne ustawienie dostępności (RES-08)"),
                  onTap: () {
                    Navigator.pop(context);
                    _changeStatus(table['id'], table['status']);
                  },
                ),
              ListTile(
                leading: const Icon(Icons.shopping_cart_checkout, color: Colors.orange),
                title: const Text("Menu i Zamawianie"),
                subtitle: const Text("Zarządzaj pozycjami dla tego stolika (RES-11/12)"),
                onTap: () {
                  Navigator.pop(context);
                  Navigator.push(context, MaterialPageRoute(builder: (_) => MenuScreen(tableId: table['id'])));
                },
              ),
              const SizedBox(height: 10),
            ],
          ),
        ),
      ),
    );
  }
}
