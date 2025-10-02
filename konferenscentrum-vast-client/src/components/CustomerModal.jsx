import { useState, useEffect } from "react";
import { useCustomers } from "../hooks/useCustomers";
import "../styles/customerModal.css";

const CustomerModal = ({ customer, onClose }) => {
  const { createCustomer, updateCustomer, loading } = useCustomers();
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phone: "",
    companyName: "", 
    address: "",
    postalCode: "",
    city: "",
  });
  const [error, setError] = useState("");

  useEffect(() => {
    if (customer) {
      setFormData({
        firstName: customer.firstName || "",
        lastName: customer.lastName || "",
        email: customer.email || "",
        phone: customer.phone || "",
        companyName: customer.companyName || "",
        address: customer.address || "",
        postalCode: customer.postalCode || "",
        city: customer.city || "",
      });
    }
  }, [customer]);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!formData.firstName || !formData.lastName || !formData.email) {
      setError("Förnamn, efternamn och e-post är obligatoriska");
      return;
    }

    try {
      if (customer) {
        await updateCustomer(customer.id, formData);
      } else {
        await createCustomer(formData);
      }
      onClose();
    } catch (err) {
      setError(err.message || "Något gick fel");
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content customer-modal-large"
        onClick={(e) => e.stopPropagation()}
      >
        <button className="modal-close" onClick={onClose}>
          ×
        </button>

        <h2>{customer ? "Redigera kund" : "Lägg till ny kund"}</h2>

        <form onSubmit={handleSubmit}>
          <div className="form-row">
            <div className="form-group">
              <label>Förnamn: *</label>
              <input
                type="text"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                placeholder="Ange förnamn"
              />
            </div>

            <div className="form-group">
              <label>Efternamn: *</label>
              <input
                type="text"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                placeholder="Ange efternamn"
              />
            </div>
          </div>

          <div className="form-group">
            <label>E-post: *</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="Ange e-post"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Telefon:</label>
              <input
                type="tel"
                name="phone"
                value={formData.phone}
                onChange={handleChange}
                placeholder="Ange telefonnummer"
              />
            </div>

            <div className="form-group">
              <label>Företag:</label>
              <input
                type="text"
                name="companyName"
                value={formData.companyName}
                onChange={handleChange}
                placeholder="Ange företagsnamn"
              />
            </div>
          </div>

          <div className="form-group">
            <label>Adress:</label>
            <input
              type="text"
              name="address"
              value={formData.address}
              onChange={handleChange}
              placeholder="Ange adress"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Postnummer:</label>
              <input
                type="text"
                name="postalCode"
                value={formData.postalCode}
                onChange={handleChange}
                placeholder="Ange postnummer"
              />
            </div>

            <div className="form-group">
              <label>Stad:</label>
              <input
                type="text"
                name="city"
                value={formData.city}
                onChange={handleChange}
                placeholder="Ange stad"
              />
            </div>
          </div>

          {error && <div className="error-message">{error}</div>}

          <div className="modal-actions">
            <button type="button" className="cancel-btn" onClick={onClose}>
              Avbryt
            </button>
            <button type="submit" className="submit-btn" disabled={loading}>
              {loading ? "Sparar..." : customer ? "Uppdatera" : "Skapa"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default CustomerModal;
