import { Link } from "react-router-dom";
import "../styles/facilityCard.css";

const FacilityCard = ({ facility }) => {
  const getMockImage = (facility) => {
    const mockImages = [
      "https://images.unsplash.com/photo-1497366216548-37526070297c?w=400&h=250&fit=crop",
      "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400&h=250&fit=crop",
      "https://images.unsplash.com/photo-1582653291997-079a1c04e5a1?w=400&h=250&fit=crop",
      "https://images.unsplash.com/photo-1556761175-4b46a572b786?w=400&h=250&fit=crop",
      "https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=400&h=250&fit=crop",
    ];

    return mockImages[facility.id % mockImages.length];
  };

  return (
    <Link to={`/facility/${facility.id}`} className="facility-card">
      <div className="facility-image">
        <img
          src={facility.imagePaths || getMockImage(facility)}
          alt={facility.name}
        />
      </div>

      <div className="facility-content">
        <h3 className="facility-name">{facility.name}</h3>
        <p className="facility-location">{facility.city}</p>
        <p className="facility-description">{facility.description}</p>

        <div className="facility-details">
          <div className="facility-capacity">
            <span className="label">Kapacitet:</span>
            <span className="value">{facility.maxCapacity} personer</span>
          </div>

          <div className="facility-price">
            <span className="label">Pris:</span>
            <span className="value">{facility.pricePerDay} kr/dag</span>
          </div>
        </div>

        <div className="facility-book-btn">Mer information</div>
      </div>
    </Link>
  );
};

export default FacilityCard;
