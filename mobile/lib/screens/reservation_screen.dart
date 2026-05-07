import 'package:flutter/material.dart';
import '../services/api_service.dart';

class ReservationScreen extends StatefulWidget {
  const ReservationScreen({super.key});
  @override
  State<ReservationScreen> createState() => _ReservationScreenState();
}

class _ReservationScreenState extends State<ReservationScreen> {
  final _apiService = ApiService();
  List<dynamic> _reservations = [];
  List<dynamic> _tables = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadAll();
  }

  void _loadAll() async {
    setState(() => _isLoading = true);
    final tablesData = await _apiService.getTables();
    final resData = await _apiService.getReservations();
    if (mounted) {
      setState(() {
        _tables = tablesData;
        _reservations = resData;
        _isLoading = false;
      });
    }
  }

  void _createRes() async {
    if (_tables.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Brak stolików w systemie.")));
      return;
    }

    int? selectedTableId;
    DateTime selectedDate = DateTime.now().add(const Duration(days: 1));

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text("Nowa Rezerwacja (RES-09)"),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text("Wybierz stolik z listy rzeczywistych stolików."),
            const SizedBox(height: 20),
            DropdownButtonFormField<int>(
              decoration: const InputDecoration(labelText: "Wybierz stolik"),
              items: _tables.map<DropdownMenuItem<int>>((t) => DropdownMenuItem(
                value: t['id'],
                child: Text("Stolik ${t['number']} (${t['status']})"),
              )).toList(),
              onChanged: (val) => selectedTableId = val,
            ),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text("ANULUJ")),
          ElevatedButton(
            onPressed: () async {
              if (selectedTableId == null) return;
              Navigator.pop(context);
              setState(() => _isLoading = true);
              bool ok = await _apiService.createReservation(selectedTableId!, selectedDate.toIso8601String());
              _loadAll(); // Odśwież listę
              if (mounted) {
                ScaffoldMessenger.of(context).showSnackBar(SnackBar(
                  content: Text(ok ? "Rezerwacja potwierdzona!" : "Błąd serwera przy rezerwacji"),
                  backgroundColor: ok ? Colors.green : Colors.redAccent,
                ));
              }
            },
            child: const Text("ZAREZERWUJ"),
          )
        ],
      ),
    );
  }

  void _delete(int id) async {
    bool ok = await _apiService.deleteReservation(id);
    if (ok) {
      _loadAll();
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Rezerwacja anulowana")));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[50],
      appBar: AppBar(
        title: const Text("Rezerwacje", style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: Colors.deepOrange,
        foregroundColor: Colors.white,
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _createRes,
        backgroundColor: Colors.deepOrange,
        label: const Text("DODAJ REZERWACJĘ", style: TextStyle(color: Colors.white)),
        icon: const Icon(Icons.add, color: Colors.white),
      ),
      body: _isLoading 
        ? const Center(child: CircularProgressIndicator())
        : _reservations.isEmpty
            ? Center(
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.calendar_today_outlined, size: 80, color: Colors.grey[300]),
                    const SizedBox(height: 20),
                    const Text("Brak aktywnych rezerwacji", style: TextStyle(color: Colors.grey, fontSize: 16)),
                  ],
                ),
              )
            : ListView.builder(
                padding: const EdgeInsets.all(16),
                itemCount: _reservations.length,
                itemBuilder: (context, index) {
                  final res = _reservations[index];
                  return Card(
                    margin: const EdgeInsets.only(bottom: 15),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
                    child: ListTile(
                      contentPadding: const EdgeInsets.all(15),
                      leading: const CircleAvatar(backgroundColor: Colors.orangeAccent, child: Icon(Icons.event, color: Colors.white)),
                      title: Text("Stolik ${res['table_id']}", style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
                      subtitle: Text("Data: ${res['reservation_date'].toString().split('T')[0]}", style: const TextStyle(color: Colors.blueGrey)),
                      trailing: IconButton(
                        icon: const Icon(Icons.cancel_outlined, color: Colors.red),
                        onPressed: () => _delete(res['id']),
                      ),
                    ),
                  );
                },
              ),
    );
  }
}
