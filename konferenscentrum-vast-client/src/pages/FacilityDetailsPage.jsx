import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { useFacilities } from "../hooks/useFacilities";
import BookingModal from "../components/BookingModal";
import "../styles/facilityDetailsPage.css";

const FacilityDetailsPage = () => {
  const { id } = useParams();
  const { facility, loading, fetchFacilityById } = useFacilities();
  const [showBookingModal, setShowBookingModal] = useState(false);

  const getMockImage = (facility) => {
    const mockImages = [
      "https://images.unsplash.com/photo-1497366216548-37526070297c?w=800&h=600&fit=crop",
      "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=800&h=600&fit=crop",
      "https://images.unsplash.com/photo-1582653291997-079a1c04e5a1?w=800&h=600&fit=crop",
      "https://images.unsplash.com/photo-1556761175-4b46a572b786?w=800&h=600&fit=crop",
      "https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=800&h=600&fit=crop",
    ];

    return mockImages[facility.id % mockImages.length];
  };

  useEffect(() => {
    fetchFacilityById(id);
  }, [id]);

  if (loading) {
    return <div className="loading">Laddar konferensrum...</div>;
  }

  if (!facility) {
    return (
      <div className="not-found">
        <h2>Konferensrummet hittades inte</h2>
        <Link to="/" className="back-btn">
          Tillbaka till översikten
        </Link>
      </div>
    );
  }

  return (
    <div className="facility-details-page">
      <Link to="/" className="back-link">
        ← Tillbaka till översikten
      </Link>

      <div className="facility-details-container">
        <div className="facility-details-image">
          <img
            src={facility.imagePaths || getMockImage(facility)}
            alt={facility.name}
          />
        </div>

        <div className="facility-details-info">
          <h1>{facility.name}</h1>
          <p className="location">{facility.city}</p>

          <div className="details-section">
            <h3>Beskrivning</h3>
            <p>{facility.description}</p>
          </div>

          <div className="details-grid">
            <div className="detail-item">
              <span className="detail-label">Kapacitet</span>
              <span className="detail-value">
                {facility.maxCapacity} personer
              </span>
            </div>
            <div className="detail-item">
              <span className="detail-label">Pris per dag</span>
              <span className="detail-value">{facility.pricePerDay} kr</span>
            </div>
          </div>

          {facility.isActive && (
            <button
              className="book-now-btn"
              onClick={() => setShowBookingModal(true)}
            >
              Boka nu
            </button>
          )}

          {!facility.isActive && (
            <div className="inactive-notice">
              Detta konferensrum är för närvarande inte tillgängligt för bokning
            </div>
          )}
        </div>
      </div>

      {showBookingModal && (
        <BookingModal
          facility={facility}
          onClose={() => setShowBookingModal(false)}
        />
      )}
    </div>
  );
};

export default FacilityDetailsPage;
