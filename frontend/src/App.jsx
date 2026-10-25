import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Navbar from './components/Navbar';
import Dashboard from './pages/Dashboard';
import UploadInspection from './pages/UploadInspection';
import PreviewConfirmation from './pages/PreviewConfirmation';
import InspectionHistory from './pages/InspectionHistory';
import InspectionDetail from './pages/InspectionDetail';
import ProductsList from './pages/ProductsList';

export default function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <main className="app-container">
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/inspect" element={<UploadInspection />} />
          <Route path="/inspect/preview/:id" element={<PreviewConfirmation />} />
          <Route path="/history" element={<InspectionHistory />} />
          <Route path="/history/:id" element={<InspectionDetail />} />
          <Route path="/products" element={<ProductsList />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
