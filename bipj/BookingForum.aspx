<%@ Page 
    Title="Workshop" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav.Master" 
    AutoEventWireup="true" 
    CodeBehind="BookingForum.aspx.cs" 
    Inherits="bipj.BookingForum" 
%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <style>
    .booking-container {
      display: flex;
      flex-wrap: wrap;
      justify-content: center;
      gap: 24px;
      padding: 32px 0;
    }
    .booking-card {
      background: #fff;
      border-radius: 12px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.1);
      overflow: hidden;
      max-width: 360px;
      width: 100%;
      display: flex;
      flex-direction: column;
      transition: transform 0.2s;
    }
    .booking-card:hover {
      transform: translateY(-4px);
    }
    .booking-card img {
      width: 100%;
      height: 200px;
      object-fit: cover;
    }
    .booking-card-body {
      padding: 20px;
      display: flex;
      flex-direction: column;
      flex: 1;
    }
    .booking-card-title {
      font-size: 1.3rem;
      font-weight: bold;
      margin-bottom: 12px;
      color: #2e266e;
    }
    .booking-card-text {
      font-size: 1rem;
      color: #555;
      margin-bottom: 20px;
      flex: 1;
    }
    .booking-btn {
      display: inline-block;
      background: #433e8e;
      color: #fff;
      border: none;
      border-radius: 6px;
      padding: 10px 20px;
      font-size: 1rem;
      text-decoration: none;
      text-align: center;
      cursor: pointer;
      transition: background 0.15s;
    }
    .booking-btn:hover {
      background: #2e266e;
    }
    @media (max-width: 768px) {
      .booking-container {
        flex-direction: column;
        align-items: center;
      }
    }
  </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <h2 style="text-align:center; margin-top:24px; color:#3b3350;">Workshop</h2>
  <div class="booking-container">
    <!-- Apply as Advisor -->
    <div class="booking-card">
      <img src="images/become_advisor.jpg" alt="Become an Advisor" />
      <div class="booking-card-body">
        <div class="booking-card-title">Become a Financial Advisor</div>
        <div class="booking-card-text">
          Share your expertise and join our team of trusted advisors.  
          Fill out a short form to get started.
        </div>
        <a href="RegisterAdvisor.aspx" class="booking-btn">Apply Now</a>
      </div>
    </div>

    <!-- Book a Session -->
    <div class="booking-card">
      <img src="images/book_session.jpg" alt="Book a Session" />
      <div class="booking-card-body">
        <div class="booking-card-title">Book a Session</div>
        <div class="booking-card-text">
          Choose an advisor, select a date &amp; time, and reserve your one-on-one or group session.
        </div>
        <a href="Booking.aspx" class="booking-btn">Book Now</a>
      </div>
    </div>
  </div>
</asp:Content>
