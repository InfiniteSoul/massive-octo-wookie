<div style="height: 300px;margin: 5px;overflow-y: auto;width: 400px;" class="scrollBar"><a class="notificationList" id=
  "notification7618512" data-ajax="1" href="/user/193019#top"><u>User</u> hat deine Freundschaftsanfrage akzeptiert.
  <div class="nDate">23.11.2016</div>
  </a><a class="nClose fa fa-times" href="javascript:;"></a><a class="notificationList" id="notification7344972" data-
  ajax="1" href="/watch/42/5/engsub#top">Lesezeichen: <u>Anime # 5</u> ist online!
  <div class="nDate">02.11.2016</div>
  <div class="nDesc"><img src="/images/hoster/mp4upload.png" alt="MP4Upload"> <img src=
  "/images/hoster/proxer-stream.png" alt="Proxer-Stream"> <img src="/images/hoster/videoweed.png" alt="VideoWeed"> <img
  src="/images/hoster/novamov.png" alt="Auroravid/Novamov"> <img src="/images/hoster/crunchyroll.png" alt=
  "Crunchyroll (EN)"> </div>
  </a><a class="nClose fa fa-times" href="javascript:;"></a></div><style>.notificationList{border: 1px solid;cursor:pointer;display:inline-block!important;min-height:50px;margin:5px 0;padding:5px;text-align:left;width:380px;}.nClose{margin:10px 5px;position:inherit;right:10px;}.nDate{font-size:10px;}.nDesc{font-site:10px;margin-top:5px;}.nDesc>img{height:15px;width:15px;}</style>
<script type="text/javascript">
	$(document).ready(function(){
		$(".scrollBar").customScrollbar();
		$('.nClose').off('click');
		$('.nClose').on('click',function(){
			var notifyId = $(this).prev().attr('id').substr('12');
			$('#notification'+notifyId).remove();
			$(this).remove();

			$.post('/notifications?format=json&s=deleteNotification', {id:notifyId},function(result){
				//
			});
		});

		$('.notificationDeleteAll').off('click');
		$('.notificationDeleteAll').on('click',function(){
			$('.notificationList').remove();
			$('.notificationDelete').remove();
			$.post('/notifications?format=json&s=deleteNotification', {id:0},function(result){
				//
			});
		});
	});
</script>