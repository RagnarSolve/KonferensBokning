import { useState, useEffect } from "react";
import { useFacilities } from "../hooks/useFacilities";
import "../styles/facilityFormModal.css";

const FacilityFormModal = ({ facility, onClose }) => {
  const { createFacility, updateFacility, loading } = useFacilities();
  const [formData, setFormData] = useState({
    name: "",
    description: "",
    address: "",
    postalCode: "",
    city: "",
    maxCapacity: "",
    pricePerDay: "",
    imagePaths: "",
    isActive: true,
  });
  const [error, setError] = useState("");

  useEffect(() => {
    if (facility) {
      setFormData({
        name: facility.name || "",
        description: facility.description || "",
        address: facility.address || "",
        postalCode: facility.postalCode || "",
        city: facility.city || "",
        maxCapacity: facility.maxCapacity || "",
        pricePerDay: facility.pricePerDay || "",
        imagePaths: facility.imagePaths || "",
        isActive: facility.isActive !== undefined ? facility.isActive : true,
      });
    }
  }, [facility]);

  const handleChange = (e) => {
    const { name, type, checked, value } = e.target;
    let finalValue;

    if (type === "checkbox") {
      finalValue = checked;
    } else if (type === "number") {
      finalValue = value === "" ? "" : parseFloat(value);
    } else {
      finalValue = value;
    }

    setFormData({
      ...formData,
      [name]: finalValue,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (
      !formData.name ||
      !formData.city ||
      !formData.maxCapacity ||
      !formData.pricePerDay
    ) {
      setError("Namn, stad, kapacitet och pris är obligatoriska");
      return;
    }

    try {
      const submitData = {
        name: formData.name,
        description: formData.description || "",
        address: formData.address || "",
        postalCode: formData.postalCode || "",
        city: formData.city,
        maxCapacity: parseInt(formData.maxCapacity),
        pricePerDay: parseFloat(formData.pricePerDay),
        isActive: formData.isActive,
      };

      // imagePaths skickas INTE till backend eftersom det inte finns i DTO:n

      if (facility) {
        await updateFacility(facility.id, submitData);
      } else {
        await createFacility(submitData);
      }
      onClose();
    } catch (err) {
      console.error("Form error:", err);
      setError(err.message || "Något gick fel");
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="modal-content facility-modal-large"
        onClick={(e) => e.stopPropagation()}
      >
        <button className="modal-close" onClick={onClose}>
          ×
        </button>

        <h2>{facility ? "Redigera facility" : "Lägg till ny facility"}</h2>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Namn: *</label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              placeholder="Konferensrummets namn"
            />
          </div>

          <div className="form-group">
            <label>Beskrivning:</label>
            <textarea
              name="description"
              value={formData.description}
              onChange={handleChange}
              placeholder="Beskriv rummet"
              rows="3"
            />
          </div>

          <div className="form-group">
            <label>Adress:</label>
            <input
              type="text"
              name="address"
              value={formData.address}
              onChange={handleChange}
              placeholder="Gatuadress"
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
                placeholder="Postnummer"
              />
            </div>

            <div className="form-group">
              <label>Stad: *</label>
              <input
                type="text"
                name="city"
                value={formData.city}
                onChange={handleChange}
                placeholder="Stad"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Max kapacitet: *</label>
              <input
                type="number"
                name="maxCapacity"
                value={formData.maxCapacity}
                onChange={handleChange}
                placeholder="Antal personer"
                min="1"
              />
            </div>

            <div className="form-group">
              <label>Pris per dag (kr): *</label>
              <input
                type="number"
                name="pricePerDay"
                value={formData.pricePerDay}
                onChange={handleChange}
                placeholder="Pris i kronor"
                min="0"
                step="0.01"
              />
            </div>
          </div>

          <div className="form-group checkbox-group">
            <label>
              <input
                type="checkbox"
                name="isActive"
                checked={formData.isActive}
                onChange={handleChange}
              />
              <span>Aktiv (synlig för kunder)</span>
            </label>
          </div>

          {error && <div className="error-message">{error}</div>}

          <div className="modal-actions">
            <button type="button" className="cancel-btn" onClick={onClose}>
              Avbryt
            </button>
            <button type="submit" className="submit-btn" disabled={loading}>
              {loading ? "Sparar..." : facility ? "Uppdatera" : "Skapa"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default FacilityFormModal;
