using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using ASP = System.Web.UI.WebControls;
using AnthemNxt.Core;

namespace AnthemNxt.Controls
{
	/// <summary>
	/// Creates an updatable multi selection check box group that can be dynamically created by binding the control to a data source. Uses callbacks if <see cref="AutoCallBack"/> is <strong>true</strong>.
	/// </summary>
	[ToolboxBitmap(typeof(ASP.CheckBoxList))]
	public class CheckBoxList : ASP.CheckBoxList, IUpdatableControl, ICallbackControl
	{
		#region Unique Anthem control code

		/// <summary>
		/// Gets or sets a value indicating whether the Checkbox state automatically calls back to
		/// the server when clicked. Mimics the AutoPostBack property.
		/// </summary>
		[DefaultValue(false)]
		public bool AutoCallBack
		{
			get
			{
				if(null == ViewState["AutoCallBack"])
					return false;
				else
					return (bool)ViewState["AutoCallBack"];
			}
			set { ViewState["AutoCallBack"] = value; }
		}

		/// <summary>
		/// Override Items collection to force PersistenceMode.InnerProperty. This will cause the control to
		/// wrap the ListItems inside of an &lt;Items&gt; tag which the Visual Studio designer will validate.
		/// If you don't do this, the designer will complain that the "Element 'ListItem' is not a known element."
		/// </summary>
		[Category("Default")]
		[DefaultValue("")]
		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public override System.Web.UI.WebControls.ListItemCollection Items
		{
			get
			{
				return base.Items;
			}
		}

		/// <summary>
		/// Adds the <strong>onclick</strong> attribute to invoke a callback from the client.
		/// </summary>
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if(AutoCallBack)
			{
				EventHandlerManager.AddScriptAttribute(
					this,
					"onclick",
					string.Format(
						"AnthemListControl_OnClick(event,{0},'{1}','{2}',{3},{4},{5},{6});",
						this.CausesValidation ? "true" : "false",
						this.ValidationGroup,
						this.TextDuringCallBack,
						this.EnabledDuringCallBack ? "true" : "false",
						(this.PreCallBackFunction == null || this.PreCallBackFunction.Length == 0) ? "null" : this.PreCallBackFunction,
						(this.PostCallBackFunction == null || this.PostCallBackFunction.Length == 0) ? "null" : this.PostCallBackFunction,
						(this.CallBackCancelledFunction == null || this.CallBackCancelledFunction.Length == 0) ? "null" : this.CallBackCancelledFunction
					)
				);
				// Disable postback so there is no double callback+postback
				ASP.CheckBox controlToRepeat = (ASP.CheckBox)this.Controls[0];
				controlToRepeat.AutoPostBack = false;
			}
		}

		/// <summary>
		/// Renders the server control wrapped in an additional element so that the
		/// element.innerHTML can be updated after a callback.
		/// </summary>
		protected override void Render(HtmlTextWriter writer)
		{
			if(!DesignMode)
			{
				AnthemNxt.Core.Manager.WriteBeginControlMarker(writer, this.RepeatLayout == ASP.RepeatLayout.Flow ? "span" : "div", this);
			}
			if(Visible)
			{
				base.Render(writer);
			}
			if(!DesignMode)
			{
				AnthemNxt.Core.Manager.WriteEndControlMarker(writer, this.RepeatLayout == ASP.RepeatLayout.Flow ? "span" : "div", this);
			}
		}

		#endregion

		#region ICallBackControl implementation

		/// <summary>
		/// Gets or sets the javascript function to execute on the client if the callback is
		/// cancelled. See <see cref="PreCallBackFunction"/>.
		/// </summary>
		[Category("Anthem")]
		[DefaultValue("")]
		[Description("The javascript function to call on the client if the callback is cancelled.")]
		public virtual string CallBackCancelledFunction
		{
			get
			{
				string text = (string)ViewState["CallBackCancelledFunction"];
				if(text == null)
					return string.Empty;
				else
					return text;
			}
			set
			{
				ViewState["CallBackCancelledFunction"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the control uses callbacks instead of postbacks to post data to the server.
		/// </summary>
		/// <value>
		/// 	<strong>true</strong> if the the control uses callbacks; otherwise,
		/// <strong>false</strong>. The default is <strong>true</strong>.
		/// </value>
		[Category("Anthem")]
		[DefaultValue(true)]
		[Description("True if this control uses callbacks instead of postbacks to post data to the server.")]
		public virtual bool EnableCallBack
		{
			get
			{
				object obj = this.ViewState["EnableCallBack"];
				if(obj == null)
					return true;
				else
					return (bool)obj;
			}
			set
			{
				ViewState["EnableCallBack"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the control is enabled on the client during callbacks.
		/// </summary>
		/// <value>
		/// 	<strong>true</strong> if the the control is enabled; otherwise,
		/// <strong>false</strong>. The default is <strong>true</strong>.
		/// </value>
		/// <remarks>Not all HTML elements support this property.</remarks>
		[Category("Anthem")]
		[DefaultValue(true)]
		[Description("True if this control is enabled on the client during callbacks.")]
		public virtual bool EnabledDuringCallBack
		{
			get
			{
				object obj = this.ViewState["EnabledDuringCallBack"];
				if(obj == null)
					return true;
				else
					return (bool)obj;
			}
			set
			{
				ViewState["EnabledDuringCallBack"] = value;
			}
		}


		/// <summary>
		/// Gets or sets the javascript function to execute on the client after the callback
		/// response is received.
		/// </summary>
		/// <remarks>
		/// The callback response is passed into the PostCallBackFunction as the one and only
		/// parameter.
		/// </remarks>
		/// <example>
		/// 	<code lang="JScript" description="This example shows a PostCallBackFunction that displays the error if there is one.">
		/// function AfterCallBack(result) {
		///   if (result.error != null &amp;&amp; result.error.length &gt; 0) {
		///     alert(result.error);
		///   }
		/// }
		///     </code>
		/// </example>
		[Category("Anthem")]
		[DefaultValue("")]
		[Description("The javascript function to call on the client after the callback response is received.")]
		public virtual string PostCallBackFunction
		{
			get
			{
				string text = (string)this.ViewState["PostCallBackFunction"];
				if(text == null)
				{
					return string.Empty;
				}
				return text;
			}
			set
			{
				ViewState["PostCallBackFunction"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the javascript function to execute on the client before the callback
		/// is made.
		/// </summary>
		/// <remarks>The function should return false on the client to cancel the callback.</remarks>
		[Category("Anthem")]
		[DefaultValue("")]
		[Description("The javascript function to call on the client before the callback is made.")]
		public virtual string PreCallBackFunction
		{
			get
			{
				string text = (string)this.ViewState["PreCallBackFunction"];
				if(text == null)
				{
					return string.Empty;
				}
				return text;
			}
			set
			{
				ViewState["PreCallBackFunction"] = value;
			}
		}

		/// <summary>Gets or sets the text to display on the client during the callback.</summary>
		/// <remarks>
		/// If the HTML element that invoked the callback has a text value (such as &lt;input
		/// type="button" value="Run"&gt;) then the text of the element is updated during the
		/// callback, otherwise the associated &lt;label&gt; text is updated during the callback.
		/// If the element does not have a text value, and if there is no associated &lt;label&gt;,
		/// then this property is ignored.
		/// </remarks>
		[Category("Anthem")]
		[DefaultValue("")]
		[Description("The text to display during the callback.")]
		public virtual string TextDuringCallBack
		{
			get
			{
				string text = (string)this.ViewState["TextDuringCallBack"];
				if(text == null)
				{
					return string.Empty;
				}
				return text;
			}
			set
			{
				ViewState["TextDuringCallBack"] = value;
			}
		}

		#endregion

		#region IUpdatableControl implementation

		/// <summary>
		/// Gets or sets a value indicating whether the control should be updated after each callback.
		/// Also see <see cref="UpdateAfterCallBack"/>.
		/// </summary>
		/// <value>
		/// 	<strong>true</strong> if the the control should be updated; otherwise,
		/// <strong>false</strong>. The default is <strong>false</strong>.
		/// </value>
		/// <example>
		/// 	<code lang="CS" description="This is normally used declaratively as shown here.">
		/// &lt;anthem:Label id="label" runat="server" AutoUpdateAfterCallBack="true" /&gt;
		///     </code>
		/// </example>
		[Category("Anthem")]
		[DefaultValue(false)]
		[Description("True if this control should be updated after each callback.")]
		public virtual bool AutoUpdateAfterCallBack
		{
			get
			{
				object obj = this.ViewState["AutoUpdateAfterCallBack"];
				if(obj == null)
					return false;
				else
					return (bool)obj;
			}
			set
			{
				if(value) UpdateAfterCallBack = true;
				ViewState["AutoUpdateAfterCallBack"] = value;
			}
		}

		private bool _updateAfterCallBack = false;

		/// <summary>
		/// Gets or sets a value which indicates whether the control should be updated after the current callback.
		/// Also see <see cref="AutoUpdateAfterCallBack"/>.
		/// </summary>
		/// <value>
		/// 	<strong>true</strong> if the the control should be updated; otherwise,
		/// <strong>false</strong>. The default is <strong>false</strong>.
		/// </value>
		/// <example>
		/// 	<code lang="CS" description="This is normally used in server code as shown here.">
		/// this.Label = "Count = " + count;
		/// this.Label.UpdateAfterCallBack = true;
		///     </code>
		/// </example>
		[Browsable(false)]
		[DefaultValue(false)]
		public virtual bool UpdateAfterCallBack
		{
			get { return _updateAfterCallBack; }
			set { _updateAfterCallBack = value; }
		}

		#endregion

		#region Common Anthem control code

		/// <summary>
		/// Raises the <see cref="System.Web.UI.Control.Load"/> event and registers the control
		/// with <see cref="AnthemNxt.Manager"/>.
		/// </summary>
		/// <param name="e">A <see cref="System.EventArgs"/>.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AnthemNxt.Core.Manager.Register(this);
		}

		/// <summary>
		/// Forces the server control to output content and trace information.
		/// </summary>
		public override void RenderControl(HtmlTextWriter writer)
		{
			base.Visible = true;
			base.RenderControl(writer);
		}

		/// <summary>
		/// Overrides the Visible property so that AnthemNxt.Manager can track the visibility.
		/// </summary>
		/// <value>
		/// 	<strong>true</strong> if the control is rendered on the client; otherwise
		/// <strong>false</strong>. The default is <strong>true</strong>.
		/// </value>
		public override bool Visible
		{
			get
			{
				return AnthemNxt.Core.Manager.GetControlVisible(this, ViewState, DesignMode);
			}
			set { AnthemNxt.Core.Manager.SetControlVisible(ViewState, value); }
		}

		#endregion
	}
}

