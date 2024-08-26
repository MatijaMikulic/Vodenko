import pyodbc
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

# Define your connection string
connection_str = (
    r'DRIVER={ODBC Driver 17 for SQL Server};'
    r'SERVER=localhost\LEVEL2;'
    r'DATABASE=Vodenko;'
    r'Trusted_Connection=yes;'
    r'TrustServerCertificate=yes;'
)

# Establish the connection
conn = pyodbc.connect(connection_str)

# Define the query to get data between 10:30 and 15:30
query = """
SELECT Theta2, Theta3, Theta4, Theta5, Theta6, dateTime
FROM ModelParameters
WHERE dateTime BETWEEN '2024-08-22 10:40:00' AND '2024-08-22 15:30:00'
"""

query = """
SELECT Theta2, Theta3, Theta4, Theta5, Theta6, dateTime
FROM ModelParameters
WHERE dateTime BETWEEN '2024-08-23 7:00:00' AND '2024-08-23 8:30:00'
"""



query = """
SELECT Theta2, Theta3, Theta4, Theta5, Theta6, dateTime
FROM ModelParameters
WHERE dateTime BETWEEN '2024-08-23 7:30:00' AND '2024-08-23 8:30:00'
"""

query = """
SELECT Theta2, Theta3, Theta4, Theta5, Theta6, dateTime
FROM ModelParameters
WHERE dateTime BETWEEN '2024-08-23 5:58:00' AND '2024-08-23 6:30:00'
"""

# Execute the query and load the data into a pandas DataFrame
df = pd.read_sql(query, conn)

# Close the connection
conn.close()

# Plotting the Theta parameters
plt.figure(figsize=(12, 8))
for col in df.columns:
    if col.startswith('Theta'):
        plt.plot(df['dateTime'], df[col], label=col)
        
plt.xlabel('Vrijeme')
plt.ylabel('Parametri')
plt.title('Vrijednosti parametara u vremenu')
plt.legend(loc='best')
plt.grid(True)
plt.show()

# Calculate time constants from the matrix A, ensuring consistent order
time_constants = []
for index, row in df.iterrows():
    # Construct the matrix A for the current row
    A = np.array([[row['Theta3'], row['Theta4']],
                  [row['Theta5'], row['Theta6']]])
    
    # Calculate the eigenvalues of A
    eigenvalues = np.linalg.eigvals(A)
    
    # Sort the eigenvalues by their real parts to ensure consistent ordering
    eigenvalues_sorted = np.sort(np.real(eigenvalues))
    
    # Calculate the time constants (negative inverse of the sorted real parts of eigenvalues)
    time_constant = -1 / eigenvalues_sorted
    #time_constants.append(time_constant)
    time_constants.append(np.append(time_constant, 1.74))


# Convert to a DataFrame for easier plotting
time_constants_df = pd.DataFrame(time_constants, columns=['Vremenska konstanta 1', 'Vremenska konstanta 2', 'Vremenska konstanta 3'])
time_constants_df['dateTime'] = df['dateTime'].values


# Plotting the time constants on a separate graph
plt.figure(figsize=(12, 8))

plt.plot(time_constants_df['dateTime'], time_constants_df['Vremenska konstanta 1'], label='Vremenska konstanta 1', color='blue')
plt.plot(time_constants_df['dateTime'], time_constants_df['Vremenska konstanta 2'], label='Vremenska konstanta 2', color='green')
plt.plot(time_constants_df['dateTime'], time_constants_df['Vremenska konstanta 3'], label='Vremenska konstanta 3', color='red')

plt.xlabel('Vrijeme')
plt.ylabel('Vremenske konstante [s]')
plt.title('Promjena vremenskih konstanti')
plt.legend(loc='best')
plt.grid(True)
plt.show()
