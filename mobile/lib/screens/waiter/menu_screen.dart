import 'package:flutter/material.dart';
import '../../services/api_service.dart';

class MenuScreen extends StatefulWidget {
  final int tableId;
  const MenuScreen({super.key, required this.tableId});
  @override
  State<MenuScreen> createState() => _MenuScreenState();
}

class _MenuScreenState extends State<MenuScreen> {
  final ApiService _apiService = ApiService();
  List<dynamic> _items = [];
  List<dynamic> _categories = [];
  int? _selectedCat;
  final List<Map<String, dynamic>> _order = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  void _load() async {
    final cats = await _apiService.getCategories();
    final items = await _apiService.getMenuItems();
    if (mounted) {
      setState(() {
        _categories = cats;
        _items = items;
        _loading = false;
      });
    }
  }

  double get _total => _order.fold(0, (sum, item) => sum + (item['price'] * item['quantity']));

  void _addItem(dynamic item) {
    TextEditingController commentCtrl = TextEditingController();
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text("Dodaj ${item['name']}"),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text("Cena: ${item['price']} PLN"),
            const SizedBox(height: 15),
            TextField(
              controller: commentCtrl,
              maxLines: 2,
              decoration: const InputDecoration(
                labelText: "Uwagi specjalne (RES-12)",
                hintText: "np. bez cebuli, bardzo ostre...",
                border: OutlineInputBorder(),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text("Anuluj")),
          ElevatedButton(
            onPressed: () {
              setState(() {
                _order.add({
                  'id': item['id'],
                  'name': item['name'],
                  'price': double.tryParse(item['price'].toString()) ?? 0.0,
                  'quantity': 1,
                  'comment': commentCtrl.text
                });
              });
              Navigator.pop(context);
            },
            child: const Text("Dodaj do zamówienia"),
          )
        ],
      ),
    );
  }

  void _submitOrder() async {
    if (_order.isEmpty) return;
    setState(() => _loading = true);
    bool ok = await _apiService.createOrder(widget.tableId, _order);
    if (mounted) {
      setState(() => _loading = false);
      if (ok) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Zamówienie wysłane do kuchni!"), backgroundColor: Colors.green));
        Navigator.pop(context);
      } else {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Nie udało się wysłać zamówienia"), backgroundColor: Colors.red));
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[50],
      appBar: AppBar(
        title: Text("Menu - Stolik ${widget.tableId}"),
        backgroundColor: Colors.deepOrange,
        foregroundColor: Colors.white,
        actions: [
          if (_order.isNotEmpty)
            Stack(
              alignment: Alignment.center,
              children: [
                IconButton(
                  icon: const Icon(Icons.receipt_long),
                  onPressed: () => _showOrderSummary(),
                ),
                Positioned(
                  right: 8,
                  top: 8,
                  child: CircleAvatar(radius: 8, backgroundColor: Colors.red, child: Text(_order.length.toString(), style: const TextStyle(fontSize: 10, color: Colors.white))),
                )
              ],
            )
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : Column(
              children: [
                // Wybór kategorii
                Container(
                  height: 60,
                  padding: const EdgeInsets.symmetric(vertical: 10),
                  child: ListView(
                    scrollDirection: Axis.horizontal,
                    padding: const EdgeInsets.symmetric(horizontal: 10),
                    children: [
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 5),
                        child: ChoiceChip(
                          label: const Text("Wszystkie"),
                          selected: _selectedCat == null,
                          onSelected: (_) => setState(() => _selectedCat = null),
                        ),
                      ),
                      ..._categories.map((c) => Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 4),
                        child: ChoiceChip(
                          label: Text(c['name']),
                          selected: _selectedCat == c['id'],
                          onSelected: (_) => setState(() => _selectedCat = c['id']),
                        ),
                      ))
                    ],
                  ),
                ),
                Expanded(
                  child: _items.isEmpty 
                    ? const Center(child: Text("Nie znaleziono pozycji menu"))
                    : ListView.builder(
                        padding: const EdgeInsets.all(12),
                        itemCount: _items.length,
                        itemBuilder: (context, i) {
                          final item = _items[i];
                          if (_selectedCat != null && item['category_id'] != _selectedCat) return const SizedBox();
                          return Card(
                            margin: const EdgeInsets.only(bottom: 12),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
                            child: ListTile(
                              contentPadding: const EdgeInsets.all(10),
                              leading: Container(
                                width: 60,
                                decoration: BoxDecoration(color: Colors.orange[50], borderRadius: BorderRadius.circular(10)),
                                child: const Icon(Icons.fastfood, color: Colors.orange),
                              ),
                              title: Text(item['name'], style: const TextStyle(fontWeight: FontWeight.bold)),
                              subtitle: Text("${item['price']} PLN", style: const TextStyle(color: Colors.deepOrange, fontWeight: FontWeight.w600)),
                              trailing: IconButton(
                                icon: const Icon(Icons.add_circle, color: Colors.green, size: 30),
                                onPressed: () => _addItem(item),
                              ),
                            ),
                          );
                        },
                      ),
                ),
                if (_order.isNotEmpty)
                  Container(
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      boxShadow: [BoxShadow(color: Colors.black.withAlpha(15), blurRadius: 10, offset: const Offset(0, -5))],
                      borderRadius: const BorderRadius.vertical(top: Radius.circular(30)),
                    ),
                    child: Column(
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            const Text("Suma (RES-17):", style: TextStyle(fontSize: 18, color: Colors.grey)),
                            Text("${_total.toStringAsFixed(2)} PLN", style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colors.deepOrange)),
                          ],
                        ),
                        const SizedBox(height: 15),
                        ElevatedButton(
                          onPressed: _submitOrder,
                          child: const Text("WYŚLIJ DO KUCHNI (RES-11)"),
                        ),
                      ],
                    ),
                  )
              ],
            ),
    );
  }

  void _showOrderSummary() {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(30))),
      builder: (context) => StatefulBuilder(
        builder: (context, setModalState) => Container(
          padding: const EdgeInsets.all(25),
          height: MediaQuery.of(context).size.height * 0.7,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text("Podsumowanie zamówienia", style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              const SizedBox(height: 20),
              Expanded(
                child: ListView.builder(
                  itemCount: _order.length,
                  itemBuilder: (context, i) {
                    final o = _order[i];
                    return ListTile(
                      title: Text(o['name'], style: const TextStyle(fontWeight: FontWeight.bold)),
                      subtitle: Text(o['comment'].isEmpty ? "Brak uwag specjalnych" : "Uwagi: ${o['comment']}"),
                      trailing: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text("${o['price']} PLN", style: const TextStyle(fontWeight: FontWeight.w600)),
                          IconButton(
                            icon: const Icon(Icons.remove_circle_outline, color: Colors.red),
                            onPressed: () {
                              setState(() => _order.removeAt(i));
                              setModalState(() {});
                              if (_order.isEmpty) Navigator.pop(context);
                            },
                          ),
                        ],
                      ),
                    );
                  },
                ),
              ),
              const Divider(),
              if (ApiService.userRole == 'guest')
                Padding(
                  padding: const EdgeInsets.only(top: 10),
                  child: OutlinedButton.icon(
                    style: OutlinedButton.styleFrom(minimumSize: const Size.fromHeight(50)),
                    icon: const Icon(Icons.payment),
                    label: const Text("Poproś o rachunek (RES-13)"),
                    onPressed: () async {
                       bool ok = await _apiService.payOrder(1); // Symulowane ID
                       if (ok && mounted) {
                         Navigator.pop(context);
                         ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text("Poproszono o rachunek!")));
                       }
                    },
                  ),
                ),
              const SizedBox(height: 10),
            ],
          ),
        ),
      ),
    );
  }
}
