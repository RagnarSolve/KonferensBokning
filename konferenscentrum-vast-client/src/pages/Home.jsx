import { useEffect } from "react";
import { useFacilities } from "../hooks/useFacilities";
import FacilityCard from "../components/FacilityCard";
import "../styles/home.css";

const Home = () => {
  const { facilities, loading, fetchActiveFacilities } = useFacilities();

  useEffect(() => {
    fetchActiveFacilities();
  }, []);

  if (loading) {
    return <div className="loading">Laddar konferensrum...</div>;
  }

  return (
    <div className="facilities-page">
      <div className="facilities-header">
        <h1>Våra Konferensrum</h1>
        <p>Hitta det perfekta rummet för ditt möte</p>
      </div>

      <div className="facilities-grid">
        {facilities.length === 0 ? (
          <div className="no-facilities">Inga konferensrum tillgängliga</div>
        ) : (
          facilities.map((facility) => (
            <FacilityCard key={facility.id} facility={facility} />
          ))
        )}
      </div>
    </div>
  );
};

export default Home;
