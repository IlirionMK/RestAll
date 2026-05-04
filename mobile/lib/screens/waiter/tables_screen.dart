import 'package:flutter/material.dart';

class TablesScreen extends StatelessWidget {
  const TablesScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('RestAll - Stoliki'),
        backgroundColor: Colors.deepOrange,
        foregroundColor: Colors.white, // Biały napis na pomarańczowym tle
      ),
      body: GridView.builder(
        padding: const EdgeInsets.all(16),
        gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2, // 2 stoliki w rzędzie
          crossAxisSpacing: 10,
          mainAxisSpacing: 10,
        ),
        itemCount: 8, // Statycznie 8 stolików do testów
        itemBuilder: (context, index) {
          bool isOccupied = index % 3 == 0;
          return Container(
            decoration: BoxDecoration(
              color: isOccupied ? Colors.red[50] : Colors.green[50],
              borderRadius: BorderRadius.circular(15),
              border: Border.all(
                color: isOccupied ? Colors.red : Colors.green,
                width: 2,
              ),
              boxShadow: const [
                BoxShadow(
                  color: Colors.black12,
                  blurRadius: 4,
                  offset: Offset(2, 2),
                )
              ],
            ),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  Icons.table_bar,
                  size: 40,
                  color: isOccupied ? Colors.red : Colors.green,
                ),
                const SizedBox(height: 8),
                Text(
                  'STOLIK ${index + 1}',
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                  decoration: BoxDecoration(
                    color: isOccupied ? Colors.red : Colors.green,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    isOccupied ? 'ZAJĘTY' : 'WOLNY',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 10,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}