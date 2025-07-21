<%@ Page Title="Review Advisor" Language="C#" MasterPageFile="~/Customer_Nav_loggedin.Master"
    AutoEventWireup="true" CodeBehind="ReviewAdvisor.aspx.cs" Inherits="bipj.ReviewAdvisor" %>

<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
  <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
  <style>
    .review-container {
      max-width: 600px;
      margin: 50px auto;
      background: #fff;
      padding: 30px;
      border-radius: 12px;
      box-shadow: 0 2px 16px rgba(0, 0, 0, 0.08);
    }

    .review-container h2 {
      text-align: center;
      color: #3b3350;
      margin-bottom: 24px;
    }

    .star-rating {
      display: flex;
      justify-content: center;
      font-size: 2rem;
      margin-bottom: 20px;
      cursor: pointer;
    }

    .star-rating i {
      color: #ccc;
      transition: color 0.3s;
    }

    .star-rating i.hovered,
    .star-rating i.selected {
      color: #f5a623;
    }

    textarea {
      width: 100%;
      height: 120px;
      padding: 10px;
      border-radius: 8px;
      border: 1px solid #ccc;
      margin-bottom: 20px;
      font-size: 14px;
    }

    .btn-submit {
      background: #5e4bd3;
      color: white;
      border: none;
      padding: 10px 24px;
      border-radius: 6px;
      font-size: 16px;
      cursor: pointer;
      display: block;
      margin: 0 auto;
    }
  </style>
</asp:Content>

<asp:Content ID="ContentBody" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <div class="review-container">
    <h2>Rate Your Advisor</h2>

    <div class="star-rating" id="starRating">
      <i class="fa fa-star" data-value="1"></i>
      <i class="fa fa-star" data-value="2"></i>
      <i class="fa fa-star" data-value="3"></i>
      <i class="fa fa-star" data-value="4"></i>
      <i class="fa fa-star" data-value="5"></i>
    </div>

    <asp:HiddenField ID="hfRating" runat="server" />
    <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Placeholder="Leave your comments here..." />
    <asp:Button ID="btnSubmit" runat="server" Text="Submit Review" CssClass="btn-submit" OnClick="btnSubmit_Click" />
  </div>

  <script>
    window.onload = function () {
      const stars = document.querySelectorAll("#starRating i");
      let currentRating = 0;
      const ratingInput = document.getElementById('<%= hfRating.ClientID %>');

      function highlightStars(rating) {
        stars.forEach(s => {
          s.classList.remove("hovered", "selected");
          if (parseInt(s.dataset.value) <= rating) {
            s.classList.add("hovered");
          }
        });
      }

      stars.forEach(star => {
        star.addEventListener("mouseover", () => {
          highlightStars(star.dataset.value);
        });

        star.addEventListener("mouseout", () => {
          highlightStars(currentRating);
        });

        star.addEventListener("click", () => {
          currentRating = parseInt(star.dataset.value);
          ratingInput.value = currentRating;
          stars.forEach(s => {
            s.classList.remove("selected");
            if (parseInt(s.dataset.value) <= currentRating) {
              s.classList.add("selected");
            }
          });
        });
      });
    };
  </script>
</asp:Content>
