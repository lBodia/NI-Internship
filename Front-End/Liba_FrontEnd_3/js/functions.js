$(document).ready(function()
{

    // Hover effects
	$(".item-m-p, .alsoList li").hover(
		function(){
			$(this).find(".image").css('opacity', 0);
			$(this).find(".additionalImage").css('opacity', 1);
		},
		function(){
			$(this).find(".image").css('opacity', 1);
			$(this).find(".additionalImage").css('opacity', 0);
	});

	// Shipping
	$("#shipping").change(function(){

		$(".shipping").remove();
		$shiping = '<div class="shipping">Shipping to ' + $(this).find("option:selected").text() + '</div>';
		
		switch( $(this).val() ) {
		  case '1':  
			$(".item-m-p").append($shiping);
		  	break;

		  case '2': 
		  	$(".item-m-p:odd").each(function(){
			    $(this).append($shiping);
			});
		  	break;

		  default:
		    break;
		}
	});

	// Change image
	$(".itemPage .previews li").click(function(){
		$src = $(this).find("img").data("big");
		$(".itemPage .mainPhoto").find("img").attr("src", $src);
	});

	// Adding to cart event
	$("#addToCart").click(function(){
		animateToCart();

		$count = parseInt($("#itemAmount").val()) + parseInt($(".smallCartCount").html());
		$(".smallCartCount").html($count);
		
	});

	// Adding item to cart animation
	function animateToCart(){  
		$left = $("#smallCart").offset().left - $(".mainPhoto").offset().left;
		$top = $("#smallCart").offset().top - $(".mainPhoto").offset().top;

        $("#mainPhoto")  
          .clone()  
          .css({'position' : 'absolute', 'top' : '0', 'z-index' : '100'})  
          .appendTo(".mainPhoto")  
          .animate({opacity: 0.5,   
                        left: $left, 
                        top: $top,
                        width: 50,   
                        height: 50}, 700, function() { $(this).remove(); });  
    }


	// Changing numbe10 of items
	$(".bfh-number").each(function(){

		$prev = '<span class="input-group-addon bfh-number-btn inc"><span class="glyphicon glyphicon-chevron-up"></span></span>';
        $next = '<span class="input-group-addon bfh-number-btn dec"><span class="glyphicon glyphicon-chevron-down"></span>';

		$(this).after($prev,$next);
	});

	$(".bfh-number-btn.inc").click(function(){
		$field = $(this).parent().find(".bfh-number");

		if($field.val() >= $field.data("max"))
		{
			return false;
		}

		$field.get(0).value++;
		priceCount();
	});

	$(".bfh-number-btn.dec").click(function(){
		$field = $(this).parent().find(".bfh-number");

		if($field.val()-1 < $field.data("min"))
		{
			return false;
		}

		$field.get(0).value--;
		priceCount();
	});


	$('.bfh-number').keyup(function () { 
	    this.value = this.value.replace(/[^0-9\.]/g,'');
	});

	$(document).on('input', '.priceinput', priceCount);

	$price = parseInt($(".priceValue .number").html());
	function priceCount()
	{
		$(".priceValue .number").html($price * $(".priceinput").val());
	}


	// Checkout
	function recountTotalPrice()
	{
		$total = 0;
		$(".itemList .item").each(function(){
			$total += parseFloat($(this).find(".itemCount").html()) * parseFloat($(this).find(".itemPrice").html());
		});
		$total -= parseFloat($(".itemList .discount").html());
		$(".itemList .total").html($total);
	}
	recountTotalPrice();
	$(".itemList .removeItem").click(function(){
		$(this).closest("tr").remove();
		if($(".itemList .item").size() == 0) nothingToOrder();
		recountTotalPrice();
	});

	function nothingToOrder(){
		$(".checkout .container").html("<h3>Корзина порожня</h3> <p>В корзині відсутні товари, оскільки ви ще не обрали жодного. </p>");
	}

	$("#makeOrder").click(function(){
		alert("No back-end");
	});



	$(".delivery input[type=radio]").change(function()
	{
		$(".radioInfo").slideUp(350);
		$(this).findNext(".radioInfo").slideDown(500);
	});
	$(".delivery input[type=radio]:checked").findNext(".radioInfo").show();
});