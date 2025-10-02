import { useState, useEffect } from "react";
import { useCustomers } from "../hooks/useCustomers";
import CustomerModal from "../components/CustomerModal";
import "../styles/customersPage.css";

const CustomersPage = () => {
  const { customers, loading, fetchAllCustomers, deleteCustomer } =
    useCustomers();
  const [showModal, setShowModal] = useState(false);
  const [editCustomer, setEditCustomer] = useState(null);
  const [searchEmail, setSearchEmail] = useState("");

  useEffect(() => {
    fetchAllCustomers();
  }, []);

  const handleEdit = (customer) => {
    setEditCustomer(customer);
    setShowModal(true);
  };

  const handleDelete = async (id) => {
    if (window.confirm("Är du säker på att du vill ta bort denna kund?")) {
      try {
        await deleteCustomer(id);
      } catch (error) {
        alert("Kunde inte ta bort kund: " + error.message);
      }
    }
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditCustomer(null);
    fetchAllCustomers();
  };

  const filteredCustomers = customers.filter((customer) => {
    if (!searchEmail) return true;
    const searchLower = searchEmail.toLowerCase();
    const firstName = (customer.firstName || "").toLowerCase();
    const lastName = (customer.lastName || "").toLowerCase();
    const fullName = `${firstName} ${lastName}`;
    const email = (customer.email || "").toLowerCase();
    return fullName.includes(searchLower) || email.includes(searchLower);
  });

  if (loading && customers.length === 0) {
    return <div className="loading">Laddar kunder...</div>;
  }

  return (
    <div className="customers-page">
      <div className="customers-header">
        <h1>Kunder</h1>
        <button className="add-btn" onClick={() => setShowModal(true)}>
          + Lägg till kund
        </button>
      </div>

      <div className="customers-search">
        <input
          type="text"
          placeholder="Sök efter namn eller e-post..."
          value={searchEmail}
          onChange={(e) => setSearchEmail(e.target.value)}
        />
      </div>

      <div className="customers-table-container">
        <table className="customers-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Namn</th>
              <th>E-post</th>
              <th>Telefon</th>
              <th>Åtgärder</th>
            </tr>
          </thead>
          <tbody>
            {filteredCustomers.length === 0 ? (
              <tr>
                <td colSpan="5" className="no-data">
                  {searchEmail ? "Inga kunder hittades" : "Inga kunder ännu"}
                </td>
              </tr>
            ) : (
              filteredCustomers
                .sort((a, b) => a.id - b.id)
                .map((customer) => (
                  <tr key={customer.id}>
                    <td>{customer.id}</td>
                    <td>
                      {`${customer.firstName || ""} ${
                        customer.lastName || ""
                      }`.trim() || "-"}
                    </td>
                    <td>{customer.email || "-"}</td>
                    <td>{customer.phone || "-"}</td>
                    <td className="actions">
                      <button
                        className="edit-btn"
                        onClick={() => handleEdit(customer)}
                      >
                        Redigera
                      </button>
                      <button
                        className="delete-btn"
                        onClick={() => handleDelete(customer.id)}
                      >
                        Ta bort
                      </button>
                    </td>
                  </tr>
                ))
            )}
          </tbody>
        </table>
      </div>

      {showModal && (
        <CustomerModal customer={editCustomer} onClose={handleCloseModal} />
      )}
    </div>
  );
};

export default CustomersPage;
