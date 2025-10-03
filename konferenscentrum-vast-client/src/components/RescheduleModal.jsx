import { useState } from "react";
import { useBookings } from "../hooks/useBookings";
import "../styles/bookingModal.css";

const RescheduleModal = ({ booking, onClose }) => {
  const { rescheduleBooking, loading } = useBookings();
  const [startDate, setStartDate] = useState(booking.startDate.split("T")[0]);
  const [endDate, setEndDate] = useState(booking.endDate.split("T")[0]);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!startDate || !endDate) {
      setError("Båda datum måste anges");
      return;
    }

    try {
      await rescheduleBooking(
        booking.id,
        startDate + "T00:00:00Z",
        endDate + "T23:59:59Z"
      );
      alert("Bokning ombokad!");
      onClose();
    } catch (err) {
      setError(err.message || "Kunde inte omboka");
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>
          ×
        </button>

        <h2>Omboka #{booking.id}</h2>
        <p className="modal-subtitle">{booking.facilityName}</p>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Nytt startdatum:</label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              min={new Date().toISOString().split("T")[0]}
            />
          </div>

          <div className="form-group">
            <label>Nytt slutdatum:</label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              min={startDate}
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <div className="modal-actions">
            <button type="button" className="cancel-btn" onClick={onClose}>
              Avbryt
            </button>
            <button type="submit" className="submit-btn" disabled={loading}>
              {loading ? "Ombokar..." : "Bekräfta ombokning"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default RescheduleModal;
